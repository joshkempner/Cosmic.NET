using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmo
{
    public class Cosmology
    {
        #region Fields

        // physical contsants in cgs units unless otherwise specified
        public const double c = 2.99792458e5; // speed of light
        public const double G = 6.67259e-8;   // gravitational constant
        public const double kmPerMpc = 3.08567758e19;
        public const double tropicalYear = 3.1556926e7; // in seconds

        // cosmological parameters
        private double _h0, _q0; // Hubble constant at z=0, q_0
        private double _omegaM, _omegaL, _omegaK; // scaled densities of matter, vacuum energy, and curvature
        private double _dH;      // Hubble distance
        private double _z;       // redshift of source

        // distance measures to source in Mpc
        private double _dA;      // angular diameter distance
        private double _dL;      // luminosity distance
        private double _dC;      // comoving line-of-sight distance
        private double _dM;      // comoving transverse distance
        private double _VC;      // comoving volume out to redshift _z in cubic Gpc

        // other quantities derived from the redshift _z
        private double _tL;      // lookback time to _z in seconds
        private double _age;     // current age of the universe in seconds
        private double _scale;   // kpc/arcsec at the redshift of the source
        private double _rhoCrit; // critical density of the universe at the redshift of the source

        // delegate used for passing functions for Romberg integration
        private delegate double Integrand(double a);

        #endregion

        #region Properties

        /// <summary>
        /// Get or set the value of the Hubble constant
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value set is &lt;= 0</exception>
        public double H0
        {
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("Hubble constant", "Value must be greater than 0");
                _h0 = value;
                SetDerivedParameters();
            }
            get { return _h0; }
        }

        /// <summary>
        /// Get or set the value of the normalized density of matter
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value set is &lt; 0</exception>
        public double OmegaMatter
        {
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Normalized matter density", "Value must be at least 0");
                _omegaM = value;
                SetDerivedParameters();
            }
            get { return _omegaM; }
        }

        /// <summary>
        /// Get or set the value of the normalized density due to a cosmological constant
        /// </summary>
        public double OmegaLambda
        {
            set
            {
                _omegaL = value;
                SetDerivedParameters();
            }
            get { return _omegaL; }
        }

        /// <summary>
        /// Normalized density due to curvature
        /// </summary>
        public double OmegaKappa
        {
            get { return _omegaK; }
        }

        /// <summary>
        /// Deceleration parameter
        /// </summary>
        public double q0
        {
            get { return _q0; }
        }

        /// <summary>
        /// Get or set the Redshift of the event
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value set is &lt; 0</exception>
        public double Redshift
        {
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Redshift", "Value must be at least 0");
                _z = value;
                SetDistances();
            }
            get { return _z; }
        }

        /// <summary>
        /// Angular diameter distance to an event at the defined redshift
        /// </summary>
        public double AngularDiameterDistance
        {
            get { return _dA; }
        }

        /// <summary>
        /// Luminosity distance to an event at the defined redshift
        /// </summary>
        public double LuminosityDistance
        {
            get { return _dL; }
        }

        /// <summary>
        /// The line-of-sight comoving distance to an event at the defined redshift
        /// </summary>
        public double ComovingDistance
        {
            get { return _dC; }
        }

        /// <summary>
        /// Transverse comoving distance. The comoving distance between two events at the defined redshift
        /// </summary>
        public double TransverseDistance
        {
            get { return _dM; }
        }

        /// <summary>
        /// The volume enclosed in a sphere centered on us and extending to an event at the defined redshift
        /// </summary>
        public double ComovingVolume
        {
            get { return _VC; }
        }

        /// <summary>
        /// The light travel time for light to reach us at z=0 from the defined redshift
        /// </summary>
        public double LookbackTime
        {
            get { return _tL; }
        }

        /// <summary>
        /// The angular scale at the defined redshift in kiloparsecs per arcsecond
        /// </summary>
        public double KpcPerArcsec
        {
            get { return _scale; }
        }

        /// <summary>
        /// The critical density required to close the Universe at the defined redshift
        /// </summary>
        public double CriticalDensity
        {
            get { return _rhoCrit; }
        }

        /// <summary>
        /// The age of the Universe at z=0
        /// </summary>
        public double Age
        {
            get { return _age; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Changes the fundamental parameters of the cosmology.
        /// Also changes the distance measures if the redshift is non-zero.
        /// </summary>
        /// <param name="h0">Hubble constant</param>
        /// <param name="omegaM">Omega-matter</param>
        /// <param name="omegaL">Omega-Lambda</param>
        public void SetCosmology(double h0, double omegaM, double omegaL)
        {
            H0 = h0;
            OmegaMatter = omegaM;
            OmegaLambda = omegaL;
            SetDerivedParameters();
        }

        /// <summary>
        /// Formats the cosmological parameters on a single line
        /// </summary>
        /// <returns>string without a line terminator at the end</returns>
        public string FormattedParameters()
        {
            return FormattedParameters("");
        }

        /// <summary>
        /// Formats the cosmological parameters on a single line
        /// </summary>
        /// <param name="leader">string to be pre-pended to the line</param>
        /// <returns>string without a line terminator at the end</returns>
        public string FormattedParameters(string leader)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}H_0 = {1}, Omega_m = {2}, Omega_L = {3}", leader, H0, OmegaMatter, OmegaLambda);
            if (Math.Abs(OmegaKappa) > double.Epsilon)
                sb.AppendFormat(", Omega_k = {0}", OmegaKappa);
            sb.AppendFormat(" (q_0 = {0})", q0);
            return sb.ToString();
        }

        /// <summary>
        /// Creates a string formatted as a header line for a text or CSV file
        /// </summary>
        /// <param name="leader">String the line starts with to mark it as a header line</param>
        /// <param name="separator">Column separator</param>
        /// <returns>string without a line terminator at the end</returns>
        public string ShortFormHeader(string leader, string separator)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(FormattedParameters(leader));
            sb.AppendFormat("{0}z{1}age{1}t_L{1}d_A{1}d_L{1}d_C{1}", leader, separator);
            if (ComovingDistance != TransverseDistance)
                sb.AppendFormat("d_T{0}", separator);
            sb.AppendFormat("V_C{0}rho_crit{0}kpc/\"{0}\"/kpc", separator);
            return sb.ToString();
        }

        /// <summary>
        /// Creates a string formatted for a text or CSV file, containing the quantities derived from the redshift
        /// </summary>
        /// <param name="separator">column separator</param>
        /// <returns>string without a line terminator at the end</returns>
        public string ShortFormOutput(string separator)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{1:f2}{0}{2:f6}{0}{3:f6}{0}{4:f6}{0}{5:f6}{0}{6:f6}{0}",
                separator, Redshift, (Age - LookbackTime) / (tropicalYear * 1e9), LookbackTime / (tropicalYear * 1e9), AngularDiameterDistance, LuminosityDistance, ComovingDistance);
            if (ComovingDistance != TransverseDistance)
                sb.AppendFormat("{1:f6}{0}", separator, TransverseDistance);
            sb.AppendFormat("{1:f6}{0}{2:e4}{0}{3:f6}{0}{4:f6}",
                separator, ComovingVolume, CriticalDensity, KpcPerArcsec, 1/KpcPerArcsec);
            return sb.ToString();
        }

        /// <summary>
        /// Creates a multi-line formatted string containing the cosmology and the quantities
        /// derived from the redshift
        /// </summary>
        /// <returns>string without a line terminator at the end</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            // get the cosmological parameters
            sb.AppendLine(FormattedParameters());

            // get the quantities derived from the redshift
            sb.AppendFormat("At z = {0}", Redshift);
            sb.AppendLine();
            sb.AppendFormat("  age of the Universe at z      = {0:f6} Gyr", (Age - LookbackTime) / (tropicalYear * 1e9));
            sb.AppendLine();
            sb.AppendFormat("  lookback time to z            = {0:f6} Gyr", LookbackTime / (tropicalYear * 1e9));
            sb.AppendLine();
            sb.AppendFormat("  angular diameter distance d_A = {0:f6} Mpc", AngularDiameterDistance);
            sb.AppendLine();
            sb.AppendFormat("  luminosity distance d_L       = {0:f6} Mpc", LuminosityDistance);
            sb.AppendLine();
            sb.AppendFormat("  comoving radial distance d_C  = {0:f6} Mpc", ComovingDistance);
            sb.AppendLine();
            if (ComovingDistance != TransverseDistance)
            {
                sb.AppendFormat("  comoving transverse distance  = {0:f6} Mpc", TransverseDistance);
                sb.AppendLine();
            }
            sb.AppendFormat("  comoving volume out to z      = {0:f6} Gpc^3", ComovingVolume);
            sb.AppendLine();
            sb.AppendFormat("  critical density at z         = {0:e6} g cm^-3", CriticalDensity);
            sb.AppendLine();
            sb.AppendFormat("  1\"  = {0:f6} kpc", KpcPerArcsec);
            sb.AppendLine();
            sb.AppendFormat("  1kpc = {0:f6} \"", 1 / KpcPerArcsec);

            return sb.ToString();
        }

        #endregion

        #region Private Methods

        // Initialize the cosmology
        private void Init(double h0, double omegaM, double omegaL)
        {
            // initialize the fundamental cosmological parameters
            _h0 = h0;
            _omegaM = omegaM;
            _omegaL = omegaL;

            // initialize the derived parameters
            SetDerivedParameters();
        }

        // Set the cosmological parameters derived from the normalized densities
        private void SetDerivedParameters()
        {
            // curvature
            _omegaK = 1.0 - _omegaM - _omegaL;
            if (_omegaK <= Double.Epsilon)
                _omegaK = 0.0;

            // other derived quantities
            _q0 = 0.5 * _omegaM - _omegaL;
            _dH = c / _h0;
            _age = Romberg(new Integrand(AgeIntegrand), 0, 1000) / _h0 * kmPerMpc;

            // initialize the redshift if needed, and always set the distance measures
            if (_z < 0)
                Redshift = 0; // use Property so distance measures get set
            SetDistances();
        }

        // Calculate the expansion factor at a given redshift
        private double E(double z)
        {
            return Math.Sqrt(_omegaM * Math.Pow(1+z, 3) + _omegaK * Math.Pow(1+z, 2) + _omegaL);
        }

        // The inverse of the expansion factor. We need this as a function so it can be used as
        // the integrand for Romberg
        private double InverseOfE(double z)
        {
            return 1.0 / E(z);
        }

        // The function that gets integrated to determine the age of the Universe
        private double AgeIntegrand(double z)
        {
            return 1.0 / (1+z) / E(z);
        }

        // Romberg integration using first argument as the integrand
        private double Romberg(Integrand integrand, double a, double b)
        {
            double h = b - a;   // coarsest panel size
            double dR;          // convergence
            double np = 1;      // current number of panels
            const int N = 25;   // maximum iterations
            double prec = 1e-8; // desired precision
            double[] R = new double[N * N];

            // compute the first term, R(0,0)
            R[0] = h / 2 * (integrand(a) + integrand(b));

            // loop over the desired number of rows, i = 2,...,N
            int i, j, k;
            for (i = 1; i < N; i++)
            {
                h /= 2.0;
                np *= 2;
                double sumT = 0.0;
                for(k = 1; k <= np-1; k += 2)
                    sumT += integrand(a + k*h);
        
                // Compute Romberg table entries R(i,1), R(i,2), ..., R(i,i)
                R[N*i] = 0.5 * R[N*(i-1)] + h * sumT;   
                int m = 1;
                for(j = 1; j < i; j++)
                {
                    m *= 4;
                    R[N*i+j] = R[N*i+j-1] + (R[N*i+j-1] - R[N*(i-1)+j-1]) / (m-1);
                }
                dR = (j > 1) ? R[N*i+j-1] - R[N*(i-1)+(j-2)] : R[0];
                if (Math.Abs(dR) < prec)
                    return R[N*i+j-1];
            }
            return R.Last();
        }

        // Calculate distance measures and other quantities that are redshift-dependent
        private void SetDistances()
        {
            // calculate critical density
            _rhoCrit = 3.0 / (8.0 * Math.PI) * Math.Pow(_h0 / kmPerMpc, 2) / G *
                (_omegaL + Math.Pow(1 + _z, 3) * _omegaM);
            
            // everything else is simple if the redshift is zero
            if (_z == 0)
            {
                _dC = _dM = _VC = _dA = _dL = _tL = 0;
                _scale = 0;
                return;
            }

            // calculate the line-of-sight comoving distance using Romberg integration
            _dC = _dH * Romberg(new Integrand(InverseOfE), 0, _z);
            
            // calculate the comoving transverse distance and comoving volume from the comoving
            // line-of-sight distance
            if (OmegaKappa > 0)
            {
                _dM = _dH / Math.Sqrt(_omegaK) * Math.Sinh(Math.Sqrt(_omegaK) * _dC / _dH);
                _VC = 2 * Math.PI * Math.Pow(_dH, 3) / _omegaK *
                    (_dM / _dH * Math.Sqrt(1 + _omegaK * Math.Pow(_dM / _dH, 3)) -
                    Asinh(Math.Abs(_omegaK) * _dM / _dH) / Math.Sqrt(_omegaK)) / 1e9;
            }
            else if (OmegaKappa < 0)
            {
                _dM = _dH / Math.Sqrt(Math.Abs(_omegaK)) * Math.Sin(Math.Sqrt(Math.Abs(_omegaK)) * _dC / _dH);
                _VC = 2 * Math.PI * Math.Pow(_dH, 3) / Math.Abs(_omegaK) *
                    (_dM / _dH * Math.Sqrt(1 + _omegaK * Math.Pow(_dM / _dH, 2)) -
                     Math.Asin(Math.Abs(_omegaK) * _dM / _dH) / Math.Sqrt(Math.Abs(_omegaK))) / 1e9;
            }
            else
            {
                _dM = _dC;
                _VC = 4 * Math.PI * Math.Pow(_dM, 3) / 3 / 1e9;
            }

            // calculate angular diameter distance and luminosity distance from the comoving transverse distance
            _dA = _dM / (1 + _z);
            _dL = _dM * (1 + _z);

            // calculate an angular scale in kpc/arcsec from the angular diameter distance
            _scale = _dA / 648 * Math.PI;

            // calculate the lookback time to the defined redshift
            _tL = Romberg(new Integrand(AgeIntegrand), 0, _z) / _h0 * kmPerMpc;
        }

        // Calculate the inverse hyperbolic sine
        private double Asinh(double p)
        {
            return Math.Log(p + Math.Sqrt(1 + p*p));
        }

        #endregion

        #region Constructors, Dispose

        /// <summary>
        /// Default constructor. Uses initial WMAP results for cosmology
        /// </summary>
        public Cosmology()
        {
            Init(71.0, 0.27, 0.73);
        }

        /// <summary>
        /// Constructor with user-specified cosmological parameters
        /// </summary>
        /// <param name="h0">Hubble constant</param>
        /// <param name="omegaM">Omega-matter</param>
        /// <param name="omegaL">Omega-Lambda</param>
        public Cosmology(double h0, double omegaM, double omegaL)
        {
            Init(h0, omegaM, omegaL);
        }
        
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="cosmology">cosmology to copy from</param>
        public Cosmology(Cosmology cosmology)
        {
            Init(cosmology.H0, cosmology.OmegaMatter, cosmology.OmegaLambda);
            Redshift = cosmology.Redshift;
        }

        #endregion
    }
}
