using OrchardCore.Comments.Models;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Media.Settings;

namespace OrchardCore.Comments;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync("CommentablePart", builder => builder
            .Attachable()
            .WithDescription("Enables comments for content items.")
        );

        await _contentDefinitionManager.AlterTypeDefinitionAsync("Comment", builder => builder
            .Creatable(false)
            .Listable(false)
            .Draftable(false)
            .Securable(false)
            .WithPart("CommentPart", part => part
                .WithPosition("0")
            )
            .WithPart("CommentText", "TextField", part => part
                .WithPosition("1")
                .WithEditor("TextArea")
                .WithSettings(new TextFieldSettings
                {
                    Hint = "Comment content",
                })
            )
            .WithPart("Attachment", "MediaField", part => part
                .WithPosition("2")
                .WithSettings(new MediaFieldSettings
                {
                    Multiple = true,
                    AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx"],
                })
            )
        );

        return 1;
    }
}
