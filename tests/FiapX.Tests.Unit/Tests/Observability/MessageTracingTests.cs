using FiapX.Infra.Observability.Messaging;
using FiapX.Tests.Unit.Helpers;
using System.Diagnostics;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace FiapX.Tests.Unit.Tests.Observability;

[TestClass]
[TestCategory("Observability")]
[TestCategory("Unit")]
[DoNotParallelize]
public class MessageTracingTests
{
    private const string QueueName = "fiapx-dev-video-processing-completed";

    [TestCleanup]
    public void Cleanup() => Activity.Current = null;

    #region Producer

    [TestMethod("Cria span de producer com as tags de messaging quando há listener ativo")]
    public void It_ShouldCreateProducerActivityWithMessagingTags_WhenListenerIsActive()
    {
        using var listener = new FiapXActivityListener();

        using var activity = MessageTracing.StartProducerActivity(QueueName);

        IsNotNull(activity);
        AreEqual($"{QueueName} publish", activity.DisplayName);
        AreEqual(ActivityKind.Producer, activity.Kind);
        AreEqual("aws_sqs", activity.GetTagItem("messaging.system"));
        AreEqual(QueueName, activity.GetTagItem("messaging.destination.name"));
    }

    [TestMethod("Retorna null ao iniciar span de producer sem listener ativo")]
    public void It_ShouldReturnNullProducerActivity_WhenNoListenerIsActive()
    {
        using var activity = MessageTracing.StartProducerActivity(QueueName);

        IsNull(activity);
    }

    #endregion

    #region Consumer

    [TestMethod("Vincula o span de consumer ao trace pai quando o traceparent é válido")]
    public void It_ShouldLinkConsumerActivityToParentTrace_WhenTraceparentIsValid()
    {
        var traceId = ActivityTraceId.CreateRandom();
        var spanId = ActivitySpanId.CreateRandom();
        var traceparent = $"00-{traceId}-{spanId}-01";
        using var listener = new FiapXActivityListener();

        using var activity = MessageTracing.StartConsumerActivity(QueueName, traceparent);

        IsNotNull(activity);
        AreEqual($"{QueueName} process", activity.DisplayName);
        AreEqual(ActivityKind.Consumer, activity.Kind);
        AreEqual(traceId, activity.TraceId);
        AreEqual(spanId.ToString(), activity.ParentSpanId.ToString());
    }

    [TestMethod("Inicia um novo trace raiz quando o traceparent é inválido")]
    public void It_ShouldStartNewRootTrace_WhenTraceparentIsInvalid()
    {
        using var listener = new FiapXActivityListener();

        using var activity = MessageTracing.StartConsumerActivity(QueueName, "not-a-valid-traceparent");

        IsNotNull(activity);
        AreEqual("0000000000000000", activity.ParentSpanId.ToString());
        AreEqual("aws_sqs", activity.GetTagItem("messaging.system"));
        AreEqual(QueueName, activity.GetTagItem("messaging.destination.name"));
    }

    [TestMethod("Inicia um novo trace raiz quando o traceparent é nulo ou vazio")]
    public void It_ShouldStartNewRootTrace_WhenTraceparentIsNullOrWhiteSpace()
    {
        using var listener = new FiapXActivityListener();

        using (var nullActivity = MessageTracing.StartConsumerActivity(QueueName, null))
        {
            IsNotNull(nullActivity);
            AreEqual("0000000000000000", nullActivity.ParentSpanId.ToString());
        }

        using (var whiteSpaceActivity = MessageTracing.StartConsumerActivity(QueueName, "   "))
        {
            IsNotNull(whiteSpaceActivity);
            AreEqual("0000000000000000", whiteSpaceActivity.ParentSpanId.ToString());
        }
    }

    #endregion

    #region CurrentTraceparent

    [TestMethod("Retorna o id da activity corrente ao consultar o traceparent atual")]
    public void It_ShouldReturnActivityId_WhenCurrentTraceparentIsCalledWithActiveActivity()
    {
        var activity = new Activity("unit-test").SetIdFormat(ActivityIdFormat.W3C).Start();

        try
        {
            AreEqual(activity.Id, MessageTracing.CurrentTraceparent());
        }
        finally
        {
            activity.Stop();
        }
    }

    [TestMethod("Retorna null ao consultar o traceparent atual sem activity ativa")]
    public void It_ShouldReturnNull_WhenCurrentTraceparentIsCalledWithoutActivity()
    {
        Activity.Current = null;

        IsNull(MessageTracing.CurrentTraceparent());
    }

    #endregion
}
