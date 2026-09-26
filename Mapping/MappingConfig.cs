using Mapster;
using learnflow_service.Dtos;
using learnflow_service.Models;

namespace learnflow_service.Mapping;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UpdateCourseRequest, Course>().IgnoreNullValues(true);
        config.NewConfig<UpdateTopicRequest, Topic>().IgnoreNullValues(true);
        config.NewConfig<UpdateTopicFolderRequest, TopicFolder>().IgnoreNullValues(true);
        config.NewConfig<UpdateNoteRequest, Note>().IgnoreNullValues(true);
        config.NewConfig<UpdateDocumentRequest, Document>().IgnoreNullValues(true);

        config.NewConfig<Attachment, AttachmentResponse>()
            .Map(dest => dest.Url, src => src.StorageUrl);

        config.NewConfig<Document, DocumentResponse>()
            .Map(dest => dest.Tags, src => src.DocumentTags.Select(dt => dt.Tag.Name).ToList());
    }
}
