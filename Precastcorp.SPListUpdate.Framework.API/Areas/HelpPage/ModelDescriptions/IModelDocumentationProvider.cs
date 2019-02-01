using System;
using System.Reflection;

namespace Precastcorp.SPListUpdate.Framework.API.Areas.HelpPage.ModelDescriptions
{
    public interface IModelDocumentationProvider
    {
        string GetDocumentation(MemberInfo member);

        string GetDocumentation(Type type);
    }
}