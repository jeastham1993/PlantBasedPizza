package com.recipe.functions.events;

public class OrderConfirmedEvent {
    private String orderIdentifier;

    public String getOrderIdentifier() {
        return orderIdentifier;
    }

    public void setOrderIdentifier(String orderIdentifier) {
        this.orderIdentifier = orderIdentifier;
    }
}
