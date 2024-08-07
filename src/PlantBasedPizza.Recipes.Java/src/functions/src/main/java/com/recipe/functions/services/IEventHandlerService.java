package com.recipe.functions.services;

import com.recipe.core.RecipeCreatedEventV1;
import com.recipe.functions.events.OrderConfirmedEvent;

public interface IEventHandlerService {
    boolean handle(OrderConfirmedEvent event);

    boolean handle (RecipeCreatedEventV1 event);
}
