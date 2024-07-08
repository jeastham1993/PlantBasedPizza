export class RecipeAdapter {
    id: string;
    name: string;
    category: string;
    price: number;
    ingredients: RecipeItemAdapter[];
}

export class RecipeItemAdapter {
    name: string;
    quantity: number;
}