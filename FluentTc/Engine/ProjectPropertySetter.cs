using System;
using EasyHttp.Http;
using FluentTc.Locators;

namespace FluentTc.Engine
{
    internal interface IProjectPropertySetter
    {
        void SetProjectParameters(Action<IBuildProjectHavingBuilder> having, Action<IBuildParameterValueBuilder> parameters);
        void DeleteProjectParameter(Action<IBuildProjectHavingBuilder> havingProject, Action<IBuildParameterHavingBuilder> parameterName);
    }

    internal class ProjectPropertySetter : IProjectPropertySetter
    {
        private readonly ITeamCityCaller m_TeamCityCaller;
        private readonly IBuildProjectHavingBuilderFactory m_BuildProjectHavingBuilderFactory;

        public ProjectPropertySetter(ITeamCityCaller teamCityCaller, IBuildProjectHavingBuilderFactory buildProjectHavingBuilderFactory)
        {
            m_TeamCityCaller = teamCityCaller;
            m_BuildProjectHavingBuilderFactory = buildProjectHavingBuilderFactory;
        }

        public void SetProjectParameters(Action<IBuildProjectHavingBuilder> having, Action<IBuildParameterValueBuilder> parameters)
        {
            var buildConfigurationHavingBuilder = m_BuildProjectHavingBuilderFactory.CreateBuildProjectHavingBuilder();
            having(buildConfigurationHavingBuilder);
            var projectLocator = buildConfigurationHavingBuilder.GetLocator();

            BuildParameterValueBuilder buildParameterValueBuilder = new BuildParameterValueBuilder();
            parameters(buildParameterValueBuilder);

            buildParameterValueBuilder.GetParameters()
                .ForEach(p => m_TeamCityCaller.PutFormat(p.Value, HttpContentTypes.TextPlain,
                        "/app/rest/projects/{0}/parameters/{1}", projectLocator, p.Name));
        }

        public void DeleteProjectParameter(Action<IBuildProjectHavingBuilder> havingProject, Action<IBuildParameterHavingBuilder> parameterName)
        {
            var buildConfigurationHavingBuilder = m_BuildProjectHavingBuilderFactory.CreateBuildProjectHavingBuilder();
            havingProject(buildConfigurationHavingBuilder);
            var projectLocator = buildConfigurationHavingBuilder.GetLocator();

            var buildParameterHavingBuilder = new BuildParameterHavingBuilder();
            parameterName(buildParameterHavingBuilder);
            var parameterNameLocator = buildParameterHavingBuilder.GetLocator();

            m_TeamCityCaller.DeleteFormat("/app/rest/projects/{0}/parameters/{1}", projectLocator, parameterNameLocator);
        }
    }
}