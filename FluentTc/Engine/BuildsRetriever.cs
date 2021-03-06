using System;
using System.Collections.Generic;
using FluentTc.Domain;
using FluentTc.Locators;

namespace FluentTc.Engine
{
    internal interface IBuildsRetriever
    {
        List<Build> GetBuilds(Action<IBuildHavingBuilder> having, Action<ICountBuilder> count,
            Action<IBuildIncludeBuilder> include);

        List<Build> GetBuildsQueue(Action<IQueueHavingBuilder> having = null);
        Build GetBuild(long buildId);
    }

    internal class BuildsRetriever : IBuildsRetriever
    {
        private readonly IBuildHavingBuilderFactory m_BuildHavingBuilderFactory;
        private readonly IBuildIncludeBuilderFactory m_BuildIncludeBuilderFactory;
        private readonly ITeamCityCaller m_Caller;
        private readonly ICountBuilderFactory m_CountBuilderFactory;
        private readonly IQueueHavingBuilderFactory m_QueueHavingBuilderFactory;

        public BuildsRetriever(ITeamCityCaller caller,
            IBuildHavingBuilderFactory buildHavingBuilderFactory,
            ICountBuilderFactory countBuilderFactory,
            IBuildIncludeBuilderFactory buildIncludeBuilderFactory, IQueueHavingBuilderFactory queueHavingBuilderFactory)
        {
            m_Caller = caller;
            m_BuildHavingBuilderFactory = buildHavingBuilderFactory;
            m_CountBuilderFactory = countBuilderFactory;
            m_BuildIncludeBuilderFactory = buildIncludeBuilderFactory;
            m_QueueHavingBuilderFactory = queueHavingBuilderFactory;
        }

        public List<Build> GetBuilds(Action<IBuildHavingBuilder> having, Action<ICountBuilder> count,
            Action<IBuildIncludeBuilder> include)
        {
            var buildHavingBuilder = m_BuildHavingBuilderFactory.CreateBuildHavingBuilder();
            having(buildHavingBuilder);
            var countBuilder = m_CountBuilderFactory.CreateCountBuilder();
            count(countBuilder);
            var buildIncludeBuilder = m_BuildIncludeBuilderFactory.CreateBuildIncludeBuilder();
            include(buildIncludeBuilder);

            var buildWrapper = GetBuildWrapper(buildHavingBuilder, countBuilder, buildIncludeBuilder);
            if (int.Parse(buildWrapper.Count) > 0)
            {
                return buildWrapper.Build;
            }
            return new List<Build>();
        }

        private BuildWrapper GetBuildWrapper(BuildHavingBuilder buildHavingBuilder, CountBuilder countBuilder,
            BuildIncludeBuilder buildIncludeBuilder)
        {
            var locator = buildHavingBuilder.GetLocator();
            var count = countBuilder.GetCount();
            var columns = buildIncludeBuilder.GetColumns();
            
            if (string.IsNullOrEmpty(count))
            {
                return m_Caller.GetFormat<BuildWrapper>("/app/rest/builds?locator={0},&fields=count,build({1})", locator, columns);
            }

            return m_Caller.GetFormat<BuildWrapper>("/app/rest/builds?locator={0},{1},&fields=count,build({2})",
                locator, count, columns);
        }

        public List<Build> GetBuildsQueue(Action<IQueueHavingBuilder> having = null)
        {
            var locator = having == null ? string.Empty : GetLocator(having);

            var buildWrapper = m_Caller.GetFormat<BuildWrapper>("/app/rest/buildQueue{0}",
                locator);
            if (int.Parse(buildWrapper.Count) > 0)
            {
                return buildWrapper.Build;
            }
            return new List<Build>();
        }

        public Build GetBuild(long buildId)
        {
            return m_Caller.GetFormat<Build>("/app/rest/builds/id:{0}", buildId);
        }

        private string GetLocator(Action<IQueueHavingBuilder> having)
        {
            var buildProjectHavingBuilder = m_QueueHavingBuilderFactory.CreateBuildProjectHavingBuilder();
            having(buildProjectHavingBuilder);
            return "?locator=" + buildProjectHavingBuilder.GetLocator();
        }
    }
}