using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using ReactiveUI;

namespace Cosmo
{
    public class Cosmology : ReactiveObject
    {
        #region Fields

        // physical constants in cgs units unless otherwise specified
        // ReSharper disable once InconsistentNaming
        private const double c = 2.99792458e5; // speed of light
        private const double G = 6.67259e-8;   // gravitational constant
        private const double KmPerMpc = 3.08567758e19;
        private const double TropicalYear = 3.1556926e7; // in seconds

        // cosmological parameters
        private double _h0;      // Hubble constant at z=0
        private double _omegaM, _omegaL; // scaled densities of matter and vacuum energy
        private double _dH;      // Hubble distance
        private double _z;       // redshift of source

        #endregion

        #region Properties

        /// <summary>
        /// Get or set the value of the Hubble constant.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value set is &lt;= 0</exception>
        [Range(double.Epsilon, double.MaxValue)]
        public double H0
        {
            get => _h0;
            set => this.RaiseAndSetIfChanged(ref _h0, value);
        }

        /// <summary>
        /// Get or set the value of the normalized density of matter.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value set is &lt; 0</exception>
        [Range(0.0, double.MaxValue)]
        public double OmegaMatter
        {
            get => _omegaM;
            set => this.RaiseAndSetIfChanged(ref _omegaM, value);
        }

        /// <summary>
        /// Get or set the value of the normalized density due to a cosmological constant.
        /// </summary>
        public double OmegaLambda
        {
            get => _omegaL;
            set => this.RaiseAndSetIfChanged(ref _omegaL, value);
        }

        /// <summary>
        /// Get the normalized density due to curvature.
        /// </summary>
        public double OmegaKappa
        {
            get;
            private set;
        }


        /// <summary>
        /// Get the deceleration parameter.
        /// </summary>
        // ReSharper disable once InconsistentNaming
#pragma warning disable IDE1006 // Naming Styles
        public double q0
#pragma warning restore IDE1006 // Naming Styles
        {
            get;
            private set;
        }

        /// <summary>
        /// Get or set the Redshift of the event.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value set is &lt; 0</exception>
        [Range(0.0, double.MaxValue)]
        public double Redshift
        {
            get => _z;
            set => this.RaiseAndSetIfChanged(ref _z, value);
        }

        /// <summary>
        /// Get the angular diameter distance (D_A) to an event at the defined redshift in Mpc.
        /// </summary>
        public double AngularDiameterDistance
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the luminosity distance (D_L)to an event at the defined redshift in Mpc.
        /// </summary>
        public double LuminosityDistance
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the line-of-sight comoving distance (D_C) to an event at the defined redshift in Mpc.
        /// </summary>
        public double ComovingDistance
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the transverse comoving distance (D_M). The comoving distance between two events at the defined redshift in Mpc.
        /// </summary>
        public double TransverseDistance
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the volume enclosed in a sphere centered on us and extending to an event at the defined redshift, in cubic Gpc.
        /// </summary>
        public double ComovingVolume
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the light travel time for light to reach us at z=0 from the defined redshift, in seconds.
        /// </summary>
        public double LookbackTime
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the angular scale at the defined redshift, in kiloparsecs per arcsecond.
        /// </summary>
        public double KpcPerArcsec
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the critical density required to close the Universe at the defined redshift, in grams per cubic centimeter.
        /// </summary>
        public double CriticalDensity
        {
            get;
            private set;
        }

        /// <summary>
        /// The age of the Universe at z=0, in seconds.
        /// </summary>
        public double Age
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Formats the cosmological parameters on a single line
        /// </summary>
        /// <param name="leader">string to be pre-pended to the line</param>
        /// <returns>string without a line terminator at the end</returns>
        public string GetFormattedParameters(string leader = "")
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0}H_0 = {1}, Omega_m = {2}, Omega_L = {3}", leader, H0, OmegaMatter, OmegaLambda);
            if (Math.Abs(OmegaKappa) > double.Epsilon)
                sb.AppendFormat(", Omega_k = {0:g3}", OmegaKappa);
            sb.AppendFormat(" (q_0 = {0})", q0);
            return sb.ToString();
        }

        /// <summary>
        /// Creates a string formatted as a header line for a text or CSV file
        /// </summary>
        /// <param name="leader">String the line starts with to mark it as a header line</param>
        /// <param name="separator">Column separator</param>
        /// <returns>string without a line terminator at the end</returns>
        public string GetShortFormHeader(string leader, string separator)
        {
            var sb = new StringBuilder();
            sb.AppendLine(GetFormattedParameters(leader));
            sb.AppendFormat("{0}z{1}age{1}t_L{1}d_A{1}d_L{1}d_C{1}", leader, separator);
            if (Math.Abs(ComovingDistance - TransverseDistance) > 1e-5)
                sb.AppendFormat("d_T{0}", separator);
            sb.AppendFormat("V_C{0}rho_crit{0}kpc/\"{0}\"/kpc", separator);
            return sb.ToString();
        }

        /// <summary>
        /// Creates a string formatted for a text or CSV file, containing the quantities derived from the redshift
        /// </summary>
        /// <param name="separator">column separator</param>
        /// <returns>string without a line terminator at the end</returns>
        public string GetShortFormOutput(string separator)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{1:f2}{0}{2:f6}{0}{3:f6}{0}{4:f6}{0}{5:f6}{0}{6:f6}{0}",
                separator, Redshift, (Age - LookbackTime) / (TropicalYear * 1e9), LookbackTime / (TropicalYear * 1e9), AngularDiameterDistance, LuminosityDistance, ComovingDistance);
            if (Math.Abs(ComovingDistance - TransverseDistance) > 1e-5)
                sb.AppendFormat("{1:f6}{0}", separator, TransverseDistance);
            sb.AppendFormat("{1:f6}{0}{2:e4}{0}{3:f6}{0}{4:f6}",
                separator, ComovingVolume, CriticalDensity, KpcPerArcsec, 1 / KpcPerArcsec);
            return sb.ToString();
        }

        /// <summary>
        /// Creates a multi-line formatted string containing the cosmology and the quantities
        /// derived from the redshift
        /// </summary>
        /// <returns>string without a line terminator at the end</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            // get the cosmological parameters
            sb.AppendLine(GetFormattedParameters());

            // get the quantities derived from the redshift
            sb.AppendFormat("At z = {0}", Redshift);
            sb.AppendLine();
            sb.AppendFormat("  age of the Universe at z      = {0:f6} Gyr", (Age - LookbackTime) / (TropicalYear * 1e9));
            sb.AppendLine();
            sb.AppendFormat("  lookback time to z            = {0:f6} Gyr", LookbackTime / (TropicalYear * 1e9));
            sb.AppendLine();
            sb.AppendFormat("  angular diameter distance d_A = {0:f6} Mpc", AngularDiameterDistance);
            sb.AppendLine();
            sb.AppendFormat("  luminosity distance d_L       = {0:f6} Mpc", LuminosityDistance);
            sb.AppendLine();
            sb.AppendFormat("  comoving radial distance d_C  = {0:f6} Mpc", ComovingDistance);
            sb.AppendLine();
            if (Math.Abs(ComovingDistance - TransverseDistance) > 1e-5)
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

        // Set the cosmological parameters derived from the normalized densities
        private void SetDerivedParameters()
        {
            // curvature
            OmegaKappa = 1.0 - _omegaM - _omegaL;
            if (Math.Abs(OmegaKappa) <= Double.Epsilon)
                OmegaKappa = 0.0;

            // other derived quantities
            q0 = 0.5 * _omegaM - _omegaL;
            _dH = c / _h0;
            Age = Romberg(AgeIntegrand, 0, 1000) / _h0 * KmPerMpc;
        }

        // Calculate the expansion factor at a given redshift
        private double E(double z)
        {
            return Math.Sqrt(_omegaM * Math.Pow(1 + z, 3) + OmegaKappa * Math.Pow(1 + z, 2) + _omegaL);
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
            return 1.0 / (1 + z) / E(z);
        }

        // Romberg integration using first argument as the integrand
        private static double Romberg(Func<double, double> integrand, double a, double b)
        {
            var h = b - a;   // coarsest panel size
            double dR;          // convergence
            double np = 1;      // current number of panels
            const int n = 25;   // maximum iterations
            const double prec = 1e-8; // desired precision
            var r = new double[n * n];

            // compute the first term, R(0,0)
            r[0] = h / 2 * (integrand(a) + integrand(b));

            // loop over the desired number of rows, i = 2,...,N
            for (var i = 1; i < n; i++)
            {
                h /= 2.0;
                np *= 2;
                var sumT = 0.0;
                int k;
                for (k = 1; k <= np - 1; k += 2)
                    sumT += integrand(a + k * h);

                // Compute Romberg table entries R(i,1), R(i,2), ..., R(i,i)
                r[n * i] = 0.5 * r[n * (i - 1)] + h * sumT;
                var m = 1;
                int j;
                for (j = 1; j < i; j++)
                {
                    m *= 4;
                    r[n * i + j] = r[n * i + j - 1] + (r[n * i + j - 1] - r[n * (i - 1) + j - 1]) / (m - 1);
                }
                dR = j > 1 ? r[n * i + j - 1] - r[n * (i - 1) + (j - 2)] : r[0];
                if (Math.Abs(dR) < prec)
                    return r[n * i + j - 1];
            }
            return r.Last();
        }

        // Calculate distance measures and other quantities that are redshift-dependent
        private void SetDistances()
        {
            var onePlusZ = 1 + _z;
            // calculate critical density
            CriticalDensity = 3.0 / (8.0 * Math.PI) * Math.Pow(_h0 / KmPerMpc, 2) / G *
                (_omegaL + Math.Pow(onePlusZ, 3) * _omegaM);

            // everything else is simple if the redshift is zero
            if (Math.Abs(_z) < 1e-5)
            {
                ComovingDistance = TransverseDistance = ComovingVolume = AngularDiameterDistance = LuminosityDistance = LookbackTime = 0;
                KpcPerArcsec = 0;
                return;
            }

            // calculate the line-of-sight comoving distance using Romberg integration
            ComovingDistance = _dH * Romberg(InverseOfE, 0, _z);

            // calculate the comoving transverse distance and comoving volume from the comoving
            // line-of-sight distance
            if (OmegaKappa > 0)
            {
                TransverseDistance = _dH / Math.Sqrt(OmegaKappa) * Math.Sinh(Math.Sqrt(OmegaKappa) * ComovingDistance / _dH);
                ComovingVolume = 2 * Math.PI * Math.Pow(_dH, 3) / OmegaKappa *
                    (TransverseDistance / _dH * Math.Sqrt(1 + OmegaKappa * Math.Pow(TransverseDistance / _dH, 2)) -
                    Asinh(Math.Sqrt(Math.Abs(OmegaKappa)) * TransverseDistance / _dH) / Math.Sqrt(Math.Abs(OmegaKappa))) / 1e9;
            }
            else if (OmegaKappa < 0)
            {
                TransverseDistance = _dH / Math.Sqrt(Math.Abs(OmegaKappa)) * Math.Sin(Math.Sqrt(Math.Abs(OmegaKappa)) * ComovingDistance / _dH);
                ComovingVolume = 2 * Math.PI * Math.Pow(_dH, 3) / OmegaKappa *
                    (TransverseDistance / _dH * Math.Sqrt(1 + OmegaKappa * Math.Pow(TransverseDistance / _dH, 2)) -
                     Math.Asin(Math.Sqrt(Math.Abs(OmegaKappa)) * TransverseDistance / _dH) / Math.Sqrt(Math.Abs(OmegaKappa))) / 1e9;
            }
            else
            {
                TransverseDistance = ComovingDistance;
                ComovingVolume = 4 * Math.PI * Math.Pow(TransverseDistance, 3) / 3 / 1e9;
            }

            // calculate angular diameter distance and luminosity distance from the comoving transverse distance
            AngularDiameterDistance = TransverseDistance / onePlusZ;
            LuminosityDistance = TransverseDistance * onePlusZ;

            // calculate an angular scale in kpc/arcsec from the angular diameter distance
            KpcPerArcsec = AngularDiameterDistance / 648 * Math.PI;

            // calculate the lookback time to the defined redshift
            LookbackTime = Romberg(AgeIntegrand, 0, _z) / _h0 * KmPerMpc;
            Console.WriteLine(ToString());
        }

        // Calculate the inverse hyperbolic sine
        private static double Asinh(double p)
        {
            return Math.Log(p + Math.Sqrt(1 + p * p));
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor. Uses initial WMAP results for cosmology
        /// </summary>
        public Cosmology()
            : this(71.0, 0.27, 0.73)
        { }

        /// <summary>
        /// Constructor with user-specified cosmological parameters
        /// </summary>
        /// <param name="h0">Hubble constant</param>
        /// <param name="omegaM">Omega-matter</param>
        /// <param name="omegaL">Omega-Lambda</param>
        public Cosmology(double h0, double omegaM, double omegaL)
        {
            H0 = h0;
            OmegaMatter = omegaM;
            OmegaLambda = omegaL;
            Redshift = 0;

            this.WhenAnyValue(
                    x => x.H0,
                    x => x.OmegaMatter,
                    x => x.OmegaLambda)
                .Subscribe(_ =>
                {
                    SetDerivedParameters();
                    SetDistances();
                });

            this.WhenAnyValue(x => x.Redshift)
                .Subscribe(_ => SetDistances());
        }

        #endregion
    }
}
