package com.recipe.functions.events;

import java.util.ArrayList;

public class OrderConfirmedEvent {
    private String orderIdentifier;
    private ArrayList<OrderConfirmedEventItem> items;

    public String getOrderIdentifier() {
        return orderIdentifier;
    }

    public void setOrderIdentifier(String orderIdentifier) {
        this.orderIdentifier = orderIdentifier;
    }

    public ArrayList<OrderConfirmedEventItem> getItems() {
        return items;
    }

    public void setItems(ArrayList<OrderConfirmedEventItem> items) {
        this.items = items;
    }
}

