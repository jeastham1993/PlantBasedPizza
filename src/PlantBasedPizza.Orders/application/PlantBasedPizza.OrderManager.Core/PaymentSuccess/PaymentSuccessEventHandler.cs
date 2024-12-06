using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.PaymentSuccess;

public class PaymentSuccessEventHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserNotificationService _notificationService;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IFeatures _features;

    public PaymentSuccessEventHandler(IOrderRepository orderRepository, IUserNotificationService notificationService,
        IWorkflowEngine workflowEngine, IFeatures features)
    {
        _orderRepository = orderRepository;
        _notificationService = notificationService;
        _workflowEngine = workflowEngine;
        _features = features;
    }

    [Channel("payments.paymentSuccessful")]
    [PublishOperation(typeof(PaymentSuccessfulEventV1), OperationId = nameof(PaymentSuccessfulEventV1))]
    public async Task Handle(PaymentSuccessfulEventV1 evt)
    {
        var order = await _orderRepository.Retrieve(evt.OrderIdentifier);

        if (_features.UseOrchestrator())
        {
            await _workflowEngine.ConfirmPayment(order.OrderIdentifier, evt.Amount);
        }
        else
        {
            order.Confirm(evt.Amount);

            await _orderRepository.Update(order);
        }

        await _notificationService.NotifyPaymentSuccess(order.CustomerIdentifier, evt.OrderIdentifier);
    }
}