﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Projac.Connector.Tests
{
    [TestFixture]
    public class ConnectedProjectorTests
    {
        [Test]
        public void ResolverCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => SutFactory((ConnectedProjectionHandlerResolver<object>)null));
        }

        [Test]
        public void ProjectAsync_ConnectionCanBeNull()
        {
            var sut = SutFactory();
            Assert.DoesNotThrow(() => sut.ProjectAsync((object)null, new object()));
        }

        [Test]
        public void ProjectAsync_MessageCanNotBeNull()
        {
            var sut = SutFactory();
            Assert.Throws<ArgumentNullException>(() => sut.ProjectAsync(new object(), (object)null));
        }

        [Test]
        public void ProjectAsyncToken_ConnectionCanBeNull()
        {
            var sut = SutFactory();
            Assert.DoesNotThrow(() => sut.ProjectAsync((object)null, new object(), CancellationToken.None));
        }

        [Test]
        public void ProjectAsyncToken_MessageCanNotBeNull()
        {
            var sut = SutFactory();
            Assert.Throws<ArgumentNullException>(() => sut.ProjectAsync(new object(), (object)null, CancellationToken.None));
        }

        [Test]
        public void ProjectAsyncMany_ConnectionCanBeNull()
        {
            var sut = SutFactory();
            Assert.DoesNotThrow(() => sut.ProjectAsync((object)null, new object[0]));
        }

        [Test]
        public void ProjectAsyncMany_MessagesCanNotBeNull()
        {
            var sut = SutFactory();
            Assert.Throws<ArgumentNullException>(() => sut.ProjectAsync(new object(), (IEnumerable<object>)null));
        }

        [Test]
        public void ProjectAsyncManyToken_ConnectionCanBeNull()
        {
            var sut = SutFactory();
            Assert.DoesNotThrow(() => sut.ProjectAsync((object)null, new object[0], CancellationToken.None));
        }

        [Test]
        public void ProjectAsyncManyToken_MessagesCanNotBeNull()
        {
            var sut = SutFactory();
            Assert.Throws<ArgumentNullException>(
                () => sut.ProjectAsync(new object(), (IEnumerable<object>) null, CancellationToken.None));
        }

        [TestCaseSource(typeof(ProjectorProjectCases), "ProjectMessageWithoutTokenCases")]
        public async void ProjectAsyncMessageCausesExpectedCalls(
            ConnectedProjectionHandlerResolver<CallRecordingConnection> resolver,
            object message,
            Tuple<int, object, CancellationToken>[] expectedCalls)
        {
            var connection = new CallRecordingConnection();
            var sut = SutFactory(resolver);

            await sut.ProjectAsync(connection, message);

            Assert.That(connection.RecordedCalls, Is.EquivalentTo(expectedCalls));
        }

        [TestCaseSource(typeof(ProjectorProjectCases), "ProjectMessageWithTokenCases")]
        public async void ProjectAsyncTokenMessageCausesExpectedCalls(
            ConnectedProjectionHandlerResolver<CallRecordingConnection> resolver,
            object message,
            CancellationToken token,
            Tuple<int, object, CancellationToken>[] expectedCalls)
        {
            var connection = new CallRecordingConnection();
            var sut = SutFactory(resolver);

            await sut.ProjectAsync(connection, message, token);

            Assert.That(connection.RecordedCalls, Is.EquivalentTo(expectedCalls));
        }

        [TestCaseSource(typeof(ProjectorProjectCases), "ProjectMessagesWithoutTokenCases")]
        public async void ProjectAsyncMessagesCausesExpectedCalls(
            ConnectedProjectionHandlerResolver<CallRecordingConnection> resolver,
            object[] messages,
            Tuple<int, object, CancellationToken>[] expectedCalls)
        {
            var connection = new CallRecordingConnection();
            var sut = SutFactory(resolver);

            await sut.ProjectAsync(connection, messages);

            Assert.That(connection.RecordedCalls, Is.EquivalentTo(expectedCalls));
        }

        [TestCaseSource(typeof(ProjectorProjectCases), "ProjectMessagesWithTokenCases")]
        public async void ProjectAsyncTokenMessagesCausesExpectedCalls(
            ConnectedProjectionHandlerResolver<CallRecordingConnection> resolver,
            object[] messages,
            CancellationToken token,
            Tuple<int, object, CancellationToken>[] expectedCalls)
        {
            var connection = new CallRecordingConnection();
            var sut = SutFactory(resolver);

            await sut.ProjectAsync(connection, messages, token);

            Assert.That(connection.RecordedCalls, Is.EquivalentTo(expectedCalls));
        }

        [Test]
        public void ProjectAsyncMessageResolverFailureCausesExpectedResult()
        {
            ConnectedProjectionHandlerResolver<object> resolver = m =>
            {
                throw new Exception("message");
            };
            var sut = SutFactory(resolver);
            var exception = Assert.Throws<Exception>(async () =>
                await sut.ProjectAsync(new object(), new object()));
            Assert.That(exception.Message, Is.EqualTo("message"));
        }

        [Test]
        public void ProjectAsyncTokenMessageResolverFailureCausesExpectedResult()
        {
            ConnectedProjectionHandlerResolver<object> resolver = m =>
            {
                throw new Exception("message");
            };
            var sut = SutFactory(resolver);
            var exception = Assert.Throws<Exception>(async () =>
                await sut.ProjectAsync(new object(), new object(), new CancellationToken()));
            Assert.That(exception.Message, Is.EqualTo("message"));
        }

        [Test]
        public void ProjectAsyncMessagesResolverFailureCausesExpectedResult()
        {
            ConnectedProjectionHandlerResolver<object> resolver = m =>
            {
                throw new Exception("message");
            };
            var sut = SutFactory(resolver);
            var exception = Assert.Throws<Exception>(async () =>
                await sut.ProjectAsync(new object(), new[] { new object(), new object() }));
            Assert.That(exception.Message, Is.EqualTo("message"));
        }

        [Test]
        public void ProjectAsyncTokenMessagesResolverFailureCausesExpectedResult()
        {
            ConnectedProjectionHandlerResolver<object> resolver = m =>
            {
                throw new Exception("message");
            };
            var sut = SutFactory(resolver);
            var exception = Assert.Throws<Exception>(async () =>
                await sut.ProjectAsync(new object(), new[] { new object(), new object() }, new CancellationToken()));
            Assert.That(exception.Message, Is.EqualTo("message"));
        }

        [Test]
        public void ProjectAsyncMessageHandlerFailureCausesExpectedResult()
        {
            Func<object, object, CancellationToken, Task> handler =
                (connection, message, token) =>
                {
                    throw new Exception("message");
                };
            ConnectedProjectionHandlerResolver<object> resolver = m => new[] {new ConnectedProjectionHandler<object>(typeof (object), handler)};
            var sut = SutFactory(resolver);
            var exception = Assert.Throws<Exception>(async () =>
                await sut.ProjectAsync(new object(), new object()));
            Assert.That(exception.Message, Is.EqualTo("message"));
        }

        [Test]
        public void ProjectAsyncTokenMessageHandlerFailureCausesExpectedResult()
        {
            Func<object, object, CancellationToken, Task> handler =
                (connection, message, token) =>
                {
                    throw new Exception("message");
                };
            ConnectedProjectionHandlerResolver<object> resolver = m => new[] { new ConnectedProjectionHandler<object>(typeof(object), handler) };
            var sut = SutFactory(resolver);
            var exception = Assert.Throws<Exception>(async () =>
                await sut.ProjectAsync(new object(), new object(), new CancellationToken()));
            Assert.That(exception.Message, Is.EqualTo("message"));
        }

        [Test]
        public void ProjectAsyncMessagesHandlerFailureCausesExpectedResult()
        {
            Func<object, object, CancellationToken, Task> handler =
                (connection, message, token) =>
                {
                    throw new Exception("message");
                };
            ConnectedProjectionHandlerResolver<object> resolver = m => new[] { new ConnectedProjectionHandler<object>(typeof(object), handler) };
            var sut = SutFactory(resolver);
            var exception = Assert.Throws<Exception>(async () =>
                await sut.ProjectAsync(new object(), new[] { new object(), new object() }));
            Assert.That(exception.Message, Is.EqualTo("message"));
        }

        [Test]
        public void ProjectAsyncTokenMessagesHandlerFailureCausesExpectedResult()
        {
            Func<object, object, CancellationToken, Task> handler =
                (connection, message, token) =>
                {
                    throw new Exception("message");
                };
            ConnectedProjectionHandlerResolver<object> resolver = m => new[] { new ConnectedProjectionHandler<object>(typeof(object), handler) };
            var sut = SutFactory(resolver);
            var exception = Assert.Throws<Exception>(async () =>
                await sut.ProjectAsync(new object(), new[] { new object(), new object() }, new CancellationToken()));
            Assert.That(exception.Message, Is.EqualTo("message"));
        }

        private static ConnectedProjector<object> SutFactory()
        {
            return SutFactory(message => new ConnectedProjectionHandler<object>[0]);
        }

        private static ConnectedProjector<TConnection> SutFactory<TConnection>(ConnectedProjectionHandlerResolver<TConnection> resolver)
        {
            return new ConnectedProjector<TConnection>(resolver);
        }
    }
}