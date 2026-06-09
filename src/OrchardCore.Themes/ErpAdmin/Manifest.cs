using OrchardCore.DisplayManagement.Manifest;
using OrchardCore.Modules.Manifest;

[assembly: Theme(
    Name = "ErpAdmin",
    Author = "ErpAdmin Team",
    Website = "https://github.com/OrchardCMS/OrchardCore",
    Version = "1.0.0",
    Description = "ERP Admin theme for OrchardCore CMS. Inherits from TheAdmin.",
    BaseTheme = "TheAdmin",
    Tags =
    [
        ManifestConstants.AdminTag,
    ]
)]
