package com.recipe.functions.services;

import com.recipe.functions.events.OrderConfirmedEvent;

public interface IEventHandlerService {
    boolean handle(OrderConfirmedEvent event);
}
