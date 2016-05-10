using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace aspnet_mvc_helpers
{
    /// <summary>
    /// Coolection of usefull helpers
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Give the date of Computer Start Date
        /// </summary>
        public static DateTime StartOfTheUnixWorld = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Compute a datetime base on ticks + start of time
        /// </summary>
        /// <param name="timestamp">Number of elapsed ticks</param>
        /// <returns>An UTC datetime</returns>
        public static DateTime FromUnixTimeStamp(this long timestamp)
        {
            var newTime = StartOfTheUnixWorld.AddSeconds(timestamp);
            return newTime;
        }

        /// <summary>
        /// Compute the number of ticks between start of time and datetime (in UTC)
        /// </summary>
        /// <param name="target">The current date & time</param>
        /// <returns>The number of ticks</returns>
        public static long ToUnixTimestamp(this DateTime target)
        {
            var unixTimestamp = Convert.ToInt64((target - StartOfTheUnixWorld).TotalSeconds);

            return unixTimestamp;
        }

        /// <summary>
        /// Convert an UTC DateTime in a correct JSON/JS string datetime (ISO 8601) 
        /// </summary>
        /// <param name="date">The datetime to convert</param>
        /// <returns>A JSON ISO 8601 datetime string </returns>
        public static String ToUtcJsISO(this DateTime date)
        {
            return new DateTime(date.Ticks, DateTimeKind.Utc).ToString("O");
        }
    }

    /// <summary>
    /// Wrapper on MD5 Hash
    /// </summary>
    public static class MD5Helpers
    {
        /// <summary>
        /// Generate a MD 5 Hash base on input text
        /// </summary>
        /// <param name="input">String to hash</param>
        /// <returns>The Hash of input</returns>
        public static String GetMd5Hash(string input)
        {
            using (var md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                var sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                foreach (var t in data)
                {
                    sBuilder.Append(t.ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }

        /// <summary>
        /// Verify a hash against a string.
        /// </summary>
        /// <param name="md5Hash">Hash to verify</param>
        /// <param name="input">Text to compare to hash</param>
        /// <param name="hash">Hash type</param>
        /// <returns>True if Hash equal to Input Hash</returns>
        public static Boolean VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            var hashOfInput = GetMd5Hash(input);

            // Create a StringComparer an compare the hashes.
            var comparer = StringComparer.OrdinalIgnoreCase;

            return 0 == comparer.Compare(hashOfInput, hash);
        }
    }

    /// <summary>
    /// Helper for culture and thread
    /// </summary>
    public static class CultureHelper
    {
        // Valid cultures
        private static readonly List<string> _validCultures = new List<string> { "af", "af-ZA", "sq", "sq-AL",
            "gsw-FR", "am-ET", "ar", "ar-DZ", "ar-BH", "ar-EG", "ar-IQ", "ar-JO", "ar-KW", "ar-LB", "ar-LY",
            "ar-MA", "ar-OM", "ar-QA", "ar-SA", "ar-SY", "ar-TN", "ar-AE", "ar-YE", "hy", "hy-AM", "as-IN", "az", "az-Cyrl-AZ", "az-Latn-AZ", "ba-RU", "eu", "eu-ES", "be", "be-BY", "bn-BD", "bn-IN", "bs-Cyrl-BA", "bs-Latn-BA", "br-FR", "bg", "bg-BG", "ca", "ca-ES", "zh-HK", "zh-MO", "zh-CN", "zh-Hans", "zh-SG", "zh-TW", "zh-Hant", "co-FR", "hr", "hr-HR", "hr-BA", "cs", "cs-CZ", "da", "da-DK", "prs-AF", "div", "div-MV", "nl", "nl-BE", "nl-NL", "en", "en-AU", "en-BZ", "en-CA", "en-029", "en-IN", "en-IE", "en-JM", "en-MY", "en-NZ", "en-PH", "en-SG", "en-ZA", "en-TT", "en-GB", "en-US", "en-ZW", "et", "et-EE", "fo", "fo-FO", "fil-PH", "fi", "fi-FI", "fr", "fr-BE", "fr-CA", "fr-FR", "fr-LU", "fr-MC", "fr-CH", "fy-NL", "gl", "gl-ES", "ka", "ka-GE", "de", "de-AT", "de-DE", "de-LI", "de-LU", "de-CH", "el", "el-GR", "kl-GL", "gu", "gu-IN", "ha-Latn-NG", "he", "he-IL", "hi", "hi-IN", "hu", "hu-HU", "is", "is-IS", "ig-NG", "id", "id-ID", "iu-Latn-CA", "iu-Cans-CA", "ga-IE", "xh-ZA", "zu-ZA", "it", "it-IT", "it-CH", "ja", "ja-JP", "kn", "kn-IN", "kk", "kk-KZ", "km-KH", "qut-GT", "rw-RW", "sw", "sw-KE", "kok", "kok-IN", "ko", "ko-KR", "ky", "ky-KG", "lo-LA", "lv", "lv-LV", "lt", "lt-LT", "wee-DE", "lb-LU", "mk", "mk-MK", "ms", "ms-BN", "ms-MY", "ml-IN", "mt-MT", "mi-NZ", "arn-CL", "mr", "mr-IN", "moh-CA", "mn", "mn-MN", "mn-Mong-CN", "ne-NP", "no", "nb-NO", "nn-NO", "oc-FR", "or-IN", "ps-AF", "fa", "fa-IR", "pl", "pl-PL", "pt", "pt-BR", "pt-PT", "pa", "pa-IN", "quz-BO", "quz-EC", "quz-PE", "ro", "ro-RO", "rm-CH", "ru", "ru-RU", "smn-FI", "smj-NO", "smj-SE", "se-FI", "se-NO", "se-SE", "sms-FI", "sma-NO", "sma-SE", "sa", "sa-IN", "sr", "sr-Cyrl-BA", "sr-Cyrl-SP", "sr-Latn-BA", "sr-Latn-SP", "nso-ZA", "tn-ZA", "si-LK", "sk", "sk-SK", "sl", "sl-SI", "es", "es-AR", "es-BO", "es-CL", "es-CO", "es-CR", "es-DO", "es-EC", "es-SV", "es-GT", "es-HN", "es-MX", "es-NI", "es-PA", "es-PY", "es-PE", "es-PR", "es-ES", "es-US", "es-UY", "es-VE", "sv", "sv-FI", "sv-SE", "syr", "syr-SY", "tg-Cyrl-TJ", "tzm-Latn-DZ", "ta", "ta-IN", "tt", "tt-RU", "te", "te-IN", "th", "th-TH", "bo-CN", "tr", "tr-TR", "tk-TM", "ug-CN", "uk", "uk-UA", "wen-DE", "ur", "ur-PK", "uz", "uz-Cyrl-UZ", "uz-Latn-UZ", "vi", "vi-VN", "cy-GB", "wo-SN", "sah-RU", "ii-CN", "yo-NG" };
        // Include ONLY cultures you are implementing
        private static List<string> _cultures = new List<string>
        {
            "en-US", // first culture is the DEFAULT
        };
        /// <summary>
        /// Returns true if the language is a right-to-left language. Otherwise, false.
        /// </summary>
        public static bool IsRighToLeft()
        {
            return Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft;

        }

        /// <summary>
        /// Set the implemented culture for app
        ///  > use it in global.asax
        /// </summary>
        /// <param name="cultures">An array of implemented cultures</param>
        public static void SetImplementedCulture(List<string> cultures)
        {
            _cultures = cultures;
        }

        /// <summary>
        /// Returns a valid culture name based on "name" parameter. If "name" is not valid, it returns the default culture "en-US"
        /// </summary>
        public static string GetImplementedCulture(string name)
        {
            // make sure it's not null
            if (string.IsNullOrEmpty(name))
                return GetDefaultCulture(); // return Default culture
                                            // make sure it is a valid culture first
            if (!_validCultures.Any(c => c.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                return GetDefaultCulture(); // return Default culture if it is invalid
                                            // if it is implemented, accept it
            if (_cultures.Any(c => c.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                return name; // accept it
                             // Find a close match. For example, if you have "en-US" defined and the user requests "en-GB", 
                             // the function will return closes match that is "en-US" because at least the language is the same (ie English)  
            var n = GetNeutralCulture(name);
            foreach (var c in _cultures)
                if (c.StartsWith(n))
                    return c;
            // else 
            // It is not implemented
            return GetDefaultCulture(); // return Default culture as no match found
        }
        /// <summary>
        /// Returns default culture name which is the first name decalared (e.g. en-US)
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultCulture()
        {
            return _cultures[0]; // return Default culture
        }
        /// <summary>
        /// Get the culture of the current thread
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentCulture()
        {
            return Thread.CurrentThread.CurrentCulture.Name;
        }
        /// <summary>
        /// Get the neutral culture of the current thread
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentNeutralCulture()
        {
            return GetNeutralCulture(Thread.CurrentThread.CurrentCulture.Name);
        }

        /// <summary>
        /// Get the neutral culture from a culture
        /// </summary>
        /// <returns></returns>
        public static string GetNeutralCulture(string name)
        {
            if (!name.Contains("-")) return name;

            return name.Split('-')[0]; // Read first part only. E.g. "en", "es"
        }
    }

    /// <summary>
    /// Represents a globally unique identifier (GUID) with a 
    /// shorter string value
    /// </summary>
    public struct ShortGuid
    {
        #region Static

        /// <summary>
        /// A read-only instance of the ShortGuid class whose value 
        /// is guaranteed to be all zeroes. 
        /// </summary>
        public static readonly ShortGuid Empty = new ShortGuid(Guid.Empty);

        #endregion

        #region Fields

        private Guid _guid;
        private string _value;

        #endregion

        #region Contructors

        /// <summary>
        /// Creates a ShortGuid from a base64 encoded string
        /// </summary>
        /// <param name="value">The encoded guid as a 
        /// base64 string</param>
        public ShortGuid(string value)
        {
            _value = value;
            _guid = Decode(value);
        }

        /// <summary>
        /// Creates a ShortGuid from a Guid
        /// </summary>
        /// <param name="guid">The Guid to encode</param>
        public ShortGuid(Guid guid)
        {
            _value = Encode(guid);
            _guid = guid;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets/sets the underlying Guid
        /// </summary>
        public Guid Guid
        {
            get { return _guid; }
            set
            {
                if (value != _guid)
                {
                    _guid = value;
                    _value = Encode(value);
                }
            }
        }

        /// <summary>
        /// Gets/sets the underlying base64 encoded string
        /// </summary>
        public string Value
        {
            get { return _value; }
            set
            {
                if (value != _value)
                {
                    _value = value;
                    _guid = Decode(value);
                }
            }
        }

        #endregion

        #region ToString

        /// <summary>
        /// Returns the base64 encoded guid as a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _value;
        }

        #endregion

        #region Equals

        /// <summary>
        /// Returns a value indicating whether this instance and a 
        /// specified Object represent the same type and value.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is ShortGuid)
                return _guid.Equals(((ShortGuid)obj)._guid);
            if (obj is Guid)
                return _guid.Equals((Guid)obj);
            if (obj is string)
                // ReSharper disable once PossibleInvalidCastException
                return _guid.Equals(((ShortGuid)obj)._guid);
            return false;
        }

        #endregion

        #region GetHashCode

        /// <summary>
        /// Returns the HashCode for underlying Guid.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return _guid.GetHashCode();
        }

        #endregion

        #region NewGuid

        /// <summary>
        /// Initialises a new instance of the ShortGuid class
        /// </summary>
        /// <returns></returns>
        public static ShortGuid NewGuid()
        {
            return new ShortGuid(Guid.NewGuid());
        }

        #endregion

        #region Encode

        /// <summary>
        /// Creates a new instance of a Guid using the string value, 
        /// then returns the base64 encoded version of the Guid.
        /// </summary>
        /// <param name="value">An actual Guid string (i.e. not a ShortGuid)</param>
        /// <returns></returns>
        public static string Encode(string value)
        {
            var guid = new Guid(value);
            return Encode(guid);
        }

        /// <summary>
        /// Encodes the given Guid as a base64 string that is 22 
        /// characters long.
        /// </summary>
        /// <param name="guid">The Guid to encode</param>
        /// <returns></returns>
        public static string Encode(Guid guid)
        {
            string encoded = Convert.ToBase64String(guid.ToByteArray());
            encoded = encoded
                .Replace("/", "_")
                .Replace("+", "-");
            return encoded.Substring(0, 22);
        }

        #endregion

        #region Decode

        /// <summary>
        /// Decodes the given base64 string
        /// </summary>
        /// <param name="value">The base64 encoded string of a Guid</param>
        /// <returns>A new Guid</returns>
        public static Guid Decode(string value)
        {
            value = value
                .Replace("_", "/")
                .Replace("-", "+");
            byte[] buffer = Convert.FromBase64String(value + "==");
            return new Guid(buffer);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if both ShortGuids have the same underlying 
        /// Guid value.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(ShortGuid x, ShortGuid y)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return x._guid == y._guid;
        }

        /// <summary>
        /// Determines if both ShortGuids do not have the 
        /// same underlying Guid value.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(ShortGuid x, ShortGuid y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implicitly converts the ShortGuid to it's string equivilent
        /// </summary>
        /// <param name="shortGuid"></param>
        /// <returns></returns>
        public static implicit operator string(ShortGuid shortGuid)
        {
            return shortGuid._value;
        }

        /// <summary>
        /// Implicitly converts the ShortGuid to it's Guid equivilent
        /// </summary>
        /// <param name="shortGuid"></param>
        /// <returns></returns>
        public static implicit operator Guid(ShortGuid shortGuid)
        {
            return shortGuid._guid;
        }

        /// <summary>
        /// Implicitly converts the string to a ShortGuid
        /// </summary>
        /// <param name="shortGuid"></param>
        /// <returns></returns>
        public static implicit operator ShortGuid(string shortGuid)
        {
            return new ShortGuid(shortGuid);
        }

        /// <summary>
        /// Implicitly converts the Guid to a ShortGuid 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static implicit operator ShortGuid(Guid guid)
        {
            return new ShortGuid(guid);
        }

        #endregion
    }

    /// <summary>
    /// Some functions to manipulate attributes and properties decoration
    /// </summary>
    public class AttributesHelpers
    {
        /// <summary>
        /// Override Class Attribute with a metadata class
        /// </summary>
        /// <example>
        ///
        /// public MyClass
        /// {
        ///    public string MyProperty {get;set;}
        /// }
        /// 
        /// public MetadataForMyClass
        /// {
        ///    [Display(Name="My class")]
        ///    public string MyProperty {get;set;}
        /// }
        /// 
        /// in your global.asax
        /// AttributesHelpers.OverrideMetaData(typeof(MyClass),typeof(MetadataForMyClass))
        /// </example>
        /// <param name="classToOverride"></param>
        /// <param name="overriderMetaData"></param>
        public static void OverrideMetaData(Type classToOverride, Type overriderMetaData)
        {
            var provider = new AssociatedMetadataTypeTypeDescriptionProvider(classToOverride, overriderMetaData);
            TypeDescriptor.AddProviderTransparent(provider, classToOverride);
        }
    }

    #region jQuery Bootgrid
    /// <summary>
    /// Parameters come from jQuery Bootgrid
    /// http://www.jquery-bootgrid.com/
    /// </summary>
    public class JBootGridParams
    {
        /// <summary>
        /// Current Page
        /// </summary>
        public int current { get; set; }
        /// <summary>
        /// Rows by page
        /// </summary>
        public int rowCount { get; set; }
        /// <summary>
        /// Dictionary of sorted column (null if no sort)
        /// </summary>
        public string sort { get; set; }
        /// <summary>
        /// Search text
        /// </summary>
        public string searchPhrase { get; set; }

    }
    /// <summary>
    /// The JS grid wants this specific structure coming back. Note their capitalization.
    /// </summary>
    public class ReturnJBootGrid<TReturn>
    {
        /// <summary>
        /// Current Page
        /// </summary>
        public int current;
        /// <summary>
        /// Rows by page
        /// </summary>
        public int rowCount;
        /// <summary>
        /// List of row to display
        /// </summary>
        public IEnumerable<TReturn> rows;
        /// <summary>
        /// Total number of rows in query
        /// </summary>
        public int total;
    }
    #endregion
}
