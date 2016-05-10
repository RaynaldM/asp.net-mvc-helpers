using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Web.Optimization;

namespace aspnet_mvc_helpers
{
    /// <summary>
    /// Builder use to make bundles
    /// from resx embedded files in dll
    /// </summary>
    public class ResourcesBundleBuilder : IBundleBuilder
    {
        private readonly string _language;
        private readonly string _javascriptName;
        private readonly string _dllName;
        private readonly string _nameSpace;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="dll">The name of DLL file</param>
        /// <param name="nameSpace">Namespace of Resources</param>
        /// <param name="jsResourcesName">Name of js resources</param>
        /// <param name="lang">Based Language</param>
        public ResourcesBundleBuilder(string dll, string nameSpace, string jsResourcesName = "rescJS", string lang = "en")
        {
            this._dllName = dll;
            this._nameSpace = nameSpace;
            this._language = lang;
            this._javascriptName = jsResourcesName;
        }

        /// <summary>
        /// JS Resources Buildr
        /// </summary>
        /// <param name="bundle">Bundle to use</param>
        /// <param name="context">Bundle context</param>
        /// <param name="files">List of files to add in bundle</param>
        /// <returns></returns>
        public string BuildBundleContent(Bundle bundle, BundleContext context, IEnumerable<BundleFile> files)
        {
            try
            {
                // Get the DLL by reflexion
                var assembly = Assembly.Load(this._dllName);
                // Try to find the resources in DLL
                var resourceSet = (new ResourceManager(this._nameSpace, assembly))
                    .GetResourceSet(new CultureInfo(this._language), true, true);

                var content = new StringBuilder(";window." + this._javascriptName + "={");
                // Add resources items in content
                foreach (DictionaryEntry entry in resourceSet)
                {
                    content.AppendFormat("{0}:\"{1}\",", entry.Key, entry.Value);
                }
                // remove the last ','
                content.Remove(content.Length-1, 1);

                // Prepare the JSON
                content.Append("};");

                return content.ToString();
            }
            catch (Exception)
            {
                return (";console.error('Resources Errors');/* Resources Errors */;");
            }
        }
    }
}
