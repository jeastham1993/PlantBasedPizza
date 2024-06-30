export class OrderConfirmedEvent {
    OrderIdentifier: string;
    Items: OrderConfirmedEventItem[];
}

export class OrderConfirmedEventItem {
    ItemName: string;
    RecipeIdentifier: string;
}