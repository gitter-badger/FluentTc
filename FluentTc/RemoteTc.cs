﻿using System;
using System.Linq;
using FluentTc.Engine;
using FluentTc.Locators;

namespace FluentTc
{
    public interface IRemoteTc
    {
        IConnectedTc Connect(Action<TeamCityConfigurationBuilder> connect);
    }

    public class RemoteTc : IRemoteTc
    {
        public IConnectedTc Connect(Action<TeamCityConfigurationBuilder> connect)
        {
            return Connect(connect, null);
        }

        internal IConnectedTc Connect(Action<TeamCityConfigurationBuilder> connect, params object[] overrides)
        {
            var teamCityConfigurationBuilder = new TeamCityConfigurationBuilder();
            connect(teamCityConfigurationBuilder);
            if (overrides == null)
            {
                overrides = new object[] {teamCityConfigurationBuilder.GetITeamCityConnectionDetails()};
            }
            else
            {
                overrides = overrides.Concat(new[] { teamCityConfigurationBuilder.GetITeamCityConnectionDetails() }).ToArray();
            }
            var bootstrapper = new Bootstrapper(overrides);
            return bootstrapper.GetConnectedTc();
        }
    }
}