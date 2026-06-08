using OrchardCore.Comments.Models;
using OrchardCore.Comments.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Comments.Drivers;

public class CommentablePartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<CommentablePart>
{
    public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
    {
        return Initialize<CommentablePartSettingsViewModel>("CommentablePartSettings_Edit", viewModel =>
        {
            var settings = contentTypePartDefinition.GetSettings<CommentablePartSettings>();
            viewModel.Settings = settings;
        })
        .Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
    {
        var viewModel = new CommentablePartSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix,
            m => m.Settings.RequireApproval,
            m => m.Settings.AllowComments,
            m => m.Settings.DefaultStatus);

        context.Builder.WithSettings(viewModel.Settings);
        return Edit(contentTypePartDefinition, context);
    }
}
