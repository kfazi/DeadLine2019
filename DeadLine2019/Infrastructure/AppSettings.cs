namespace DeadLine2019.Infrastructure
{
    using Westwind.Utilities.Configuration;

    public class AppSettings : AppConfiguration
    {
        public static AppSettings Create()
        {
            var appSettings = new AppSettings();

            appSettings.Initialize();

            return appSettings;
        }

        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            var provider = new ConfigurationFileConfigurationProvider<AppSettings>();
            return provider;
        }
    }
}