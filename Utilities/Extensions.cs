namespace UpdateManager.CoreLibrary.Utilities;
public static class Extensions
{
    extension (INugetModel model)
    {
        public string RepositoryDirectory
        {
            get
            {
                string output = Path.GetDirectoryName(model.CsProjPath)!;
                return output;
            }
        }
    }
    extension (IVersionable package)
    {
        public string PackageID
        {
            get
            {
                if (string.IsNullOrWhiteSpace(package.PrefixForPackageName))
                {
                    return package.PackageName;
                }
                return $"{package.PrefixForPackageName}.{package.PackageName}";
            }
        }
    }   
    extension (IConfiguration configuration)
    {
        public string UploadedPackagesStoragePathFromConfig
        {
            get
            {
                // Retrieve the storage path for uploaded packages from the configuration
                var value = configuration[UpdateSystemConfigurationKeys.UploadedPackagesStoragePathKey];
                // If the key is not found, throw an exception
                if (string.IsNullOrEmpty(value))
                {
                    throw new ConfigurationKeyNotFoundException("The uploaded packages storage path is not registered in the configuration.");
                }
                return value;
            }
        }
        public string UploadedToolsStoragePathFromConfig
        {
            get
            {
                // Retrieve the storage path for uploaded tools from the configuration
                var value = configuration[UpdateSystemConfigurationKeys.UploadedToolsStoragePathKey];
                // If the key is not found, throw an exception
                if (string.IsNullOrEmpty(value))
                {
                    throw new ConfigurationKeyNotFoundException("The uploaded tools storage path is not registered in the configuration.");
                }
                return value;
            }
        }
        public string UploadedTemplatesStoragePathFromConfig
        {
            get
            {
                // Retrieve the storage path for uploaded templates from the configuration
                var value = configuration[UpdateSystemConfigurationKeys.UploadedTemplatesStoragePathKey];
                // If the key is not found, throw an exception
                if (string.IsNullOrEmpty(value))
                {
                    throw new ConfigurationKeyNotFoundException("The uploaded templates storage path is not registered in the configuration.");
                }
                return value;

            }
        }
        public string PackagePostBuildFeedProcessorProgram
        {
            get
            {
                var value = configuration[UpdateSystemConfigurationKeys.PostBuildFeedProcessorKey_Packages];

                // If the key is not found, throw an exception
                if (string.IsNullOrEmpty(value))
                {
                    throw new ConfigurationKeyNotFoundException("The post program is not registered in the configuration.");
                }
                return value;
            }   
        }
        public string ToolPostBuildFeedProcessorProgram
        {
            get
            {
                var value = configuration[UpdateSystemConfigurationKeys.PostBuildFeedProcessorKey_Tools];
                if (string.IsNullOrEmpty(value))
                {
                    throw new ConfigurationKeyNotFoundException("The tool post-build processor is not registered in the configuration.");
                }

                return value;
            }
        }
        public string PackagePrefixFromConfig
        {
            get
            {
                var value = configuration[UpdateSystemConfigurationKeys.PrefixKey];

                // If the key is not found, throw an exception
                if (string.IsNullOrEmpty(value))
                {
                    throw new ConfigurationKeyNotFoundException("The package prefix key is not registered in the configuration.");
                }
                return value;
            }   
        }
        public string DevelopmentPackagePath
        {
            get
            {
                var value = configuration[UpdateSystemConfigurationKeys.DevelopmentPackageKey];

                // If the key is not found, throw an exception
                if (string.IsNullOrEmpty(value))
                {
                    throw new ConfigurationKeyNotFoundException("The development nuget package feed path key is not registered in the configuration.");
                }
                return value;
            }
        }
        public string StagingPackagePath
        {
            get
            {
                var value = configuration[UpdateSystemConfigurationKeys.StagingPackageKey];

                // If the key is not found, throw an exception
                if (string.IsNullOrEmpty(value))
                {
                    throw new ConfigurationKeyNotFoundException("The staging nuget package feed path key is not registered in the configuration.");
                }
                return value;
            }   
        }
        public string PrivatePackagePath
        {
            get
            {
                var value = configuration[UpdateSystemConfigurationKeys.PrivatePackageKey];

                // If the key is not found, throw an exception
                if (string.IsNullOrEmpty(value))
                {
                    throw new ConfigurationKeyNotFoundException("The private nuget package feed path key is not registered in the configuration.");
                }
                return value;
            }
        }
        public string NugetConfigPath
        {
            get
            {
                var value = configuration[UpdateSystemConfigurationKeys.PackageConfigFileKey];

                // If the key is not found, throw an exception
                if (string.IsNullOrEmpty(value))
                {
                    throw new ConfigurationKeyNotFoundException("The NuGet config path is not registered in the configuration.");
                }
                return value;
            }   
        }
        public string RequiredNuGetPackagesPath
        {
            get
            {
                // Check if the configuration key is found
                var value = configuration[UpdateSystemConfigurationKeys.CustomPackageInfoKey];

                // If the key is not found, throw an exception
                if (string.IsNullOrEmpty(value))
                {
                    throw new ConfigurationKeyNotFoundException("The NuGet package path key is not registered in the configuration.");
                }
                return value;
            }
        }
        public string RequiredNuGetToolPath
        {
            get
            {
                // Check if the configuration key is found
                var value = configuration[UpdateSystemConfigurationKeys.CustomToolInfoKey];
                // If the key is not found, throw an exception
                if (string.IsNullOrEmpty(value))
                {
                    throw new ConfigurationKeyNotFoundException("The Custom Tool path key is not registered in the configuration.");
                }
                return value;
            }
        }
        public string RequiredNuGetTemplatePath
        {
            get
            {
                // Check if the configuration key is found
                var value = configuration[UpdateSystemConfigurationKeys.CustomTemplateInfoKey];
                // If the key is not found, throw an exception
                if (string.IsNullOrEmpty(value))
                {
                    throw new ConfigurationKeyNotFoundException("The NuGet template path key is not registered in the configuration.");
                }
                return value;
            }
        }
        public string RequiredAppPackagesPath
        {
            get
            {
                var value = configuration[UpdateSystemConfigurationKeys.AppInfoKey];

                // If the key is not found, throw an exception
                if (string.IsNullOrEmpty(value))
                {
                    throw new ConfigurationKeyNotFoundException("The App path key is not registered in the configuration.");
                }
                return value;
            }
        }  
    }
    extension (string version)
    {
        // Extract the major version from version string
        public int MajorVersion
        {
            get
            {
                var versionParts = version.Split('.');
                if (versionParts.Length < 1)
                {
                    throw new CustomBasicException("Invalid version format. Major version is required.");
                }
                return int.Parse(versionParts[0]);
            }
            
        }
        // Extract the minor version (the last part of the version string)
        public int MinorVersion
        {
            get
            {
                var versionParts = version.Split('.');
                if (versionParts.Length < 1)
                {
                    throw new CustomBasicException("Invalid version format. Minor version is required.");
                }

                // The very last part should always be the minor version, regardless of how many parts are in the version string
                return int.Parse(versionParts.Last());
            }
        }
        // Increment the minor version and maintain the major.0.minor format
        public string IncrementMinorVersion()
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                throw new CustomBasicException("Version string is empty or null, unable to increment minor version.");
            }

            var versionParts = version.Split('.');
            // Ensure that the version has at least 2 parts (major.minor format)
            if (versionParts.Length < 2)
            {
                throw new CustomBasicException("Invalid version format. Version should have at least 'major.minor' format.");
            }
            // Extract major version (the first part)
            int major = int.Parse(versionParts[0]);

            // Always take the last part as the minor version, no matter how many parts are there
            int minor = int.Parse(versionParts.Last());

            // Increment minor version
            minor++;

            // Return in major.0.minor format
            return $"{major}.0.{minor}";
        }
        public string StartMajorVersion()
        {
            var versionParts = version.Split('.');
            // Ensure that the version has at least 2 parts (major.minor format)
            if (versionParts.Length < 2)
            {
                throw new CustomBasicException("Invalid version format. Version should have at least 'major.minor' format.");
            }
            // Extract major version (the first part)
            int major = int.Parse(versionParts[0]);

            return $"{major}.0.0"; //try this way.   since i am forced to make this build, then just make this .0.0 and when the build happens, will increment by one properly.
                                   //this means go ahead and let the normal builds happen anyways.
        }
    }

}