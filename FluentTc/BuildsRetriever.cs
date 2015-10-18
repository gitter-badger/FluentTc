using System;
using System.Collections.Generic;
using FluentTc.Domain;
using FluentTc.Locators;

namespace FluentTc
{
    internal interface IBuildsRetriever
    {
        List<Build> GetBuilds(Action<BuildHavingBuilder> having, Action<CountBuilder> count, Action<BuildIncludeBuilder> include);
    }

    internal class BuildsRetriever : IBuildsRetriever
    {
        private readonly ITeamCityCaller m_Caller;
        private readonly IBuildHavingBuilderFactory m_BuildHavingBuilderFactory;

        public BuildsRetriever(ITeamCityCaller caller, IBuildHavingBuilderFactory buildHavingBuilderFactory)
        {
            m_Caller = caller;
            m_BuildHavingBuilderFactory = buildHavingBuilderFactory;
        }

        public List<Build> GetBuilds(Action<BuildHavingBuilder> having, Action<CountBuilder> count, Action<BuildIncludeBuilder> include)
        {
            var buildHavingBuilder = m_BuildHavingBuilderFactory.CreateBuildHavingBuilder();
            having(buildHavingBuilder);
            var buildIncludeBuilder = new BuildIncludeBuilder();
            include(buildIncludeBuilder);
            var countBuilder = new CountBuilder();
            count(countBuilder);

            var locator = buildHavingBuilder.GetLocator();
            var parts = countBuilder.GetCount();
            var columns = buildIncludeBuilder.GetColumns();
            var buildWrapper = m_Caller.GetFormat<BuildWrapper>("/app/rest/builds?locator={0},count:{1},&fields=count,build({2})",
                locator, parts, columns);
            if (int.Parse(buildWrapper.Count) > 0)
            {
                return buildWrapper.Build;
            }
            return new List<Build>();
        }
    }
}