﻿using System.Collections.Generic;
using System.Diagnostics;
using Ocelot.LoadBalancer.LoadBalancers;
using Ocelot.Responses;
using Ocelot.Values;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.LoadBalancer
{
    public class RoundRobinTests
    {
        private readonly RoundRobinLoadBalancer _roundRobin;
        private readonly List<Service> _services;
        private Response<HostAndPort> _hostAndPort;

        public RoundRobinTests()
        {
            _services = new List<Service>
            {
                new Service("product", new HostAndPort("127.0.0.1", 5000), string.Empty, string.Empty, new string[0]),
                new Service("product", new HostAndPort("127.0.0.1", 5001), string.Empty, string.Empty, new string[0]),
                new Service("product", new HostAndPort("127.0.0.1", 5001), string.Empty, string.Empty, new string[0])
            };

            _roundRobin = new RoundRobinLoadBalancer(_services);
        }

        [Fact]
        public void should_get_next_address()
        {
            this.Given(x => x.GivenIGetTheNextAddress())
                .Then(x => x.ThenTheNextAddressIndexIs(0))
                .Given(x => x.GivenIGetTheNextAddress())
                .Then(x => x.ThenTheNextAddressIndexIs(1))
                .Given(x => x.GivenIGetTheNextAddress())
                .Then(x => x.ThenTheNextAddressIndexIs(2))
                .BDDfy();
        }

        [Fact]
        public void should_go_back_to_first_address_after_finished_last()
        {
            var stopWatch = Stopwatch.StartNew();

            while (stopWatch.ElapsedMilliseconds < 1000)
            {
                var address = _roundRobin.Lease().Result;
                address.Data.ShouldBe(_services[0].HostAndPort);
                address = _roundRobin.Lease().Result;
                address.Data.ShouldBe(_services[1].HostAndPort);
                address = _roundRobin.Lease().Result;
                address.Data.ShouldBe(_services[2].HostAndPort);
            }
        }

        private void GivenIGetTheNextAddress()
        {
            _hostAndPort = _roundRobin.Lease().Result;
        }

        private void ThenTheNextAddressIndexIs(int index)
        {
            _hostAndPort.Data.ShouldBe(_services[index].HostAndPort);
        }
    }
}
