// seeds/beverage_seed.js
exports.seed = function(knex) {
  return knex('BEVERAGE').del()
    .then(function() {
      return knex('BEVERAGE').insert([
        { ID: 1, BEVERAGE_NAME: "Latte", IMAGE_PATH: "/Assets/latte.jpg", CATEGORY_ID: 1, STATUS: 1 },
        { ID: 2, BEVERAGE_NAME: "Green Tea", IMAGE_PATH: "/Assets/green_tea.jpg", CATEGORY_ID: 2, STATUS: 1 },
        { ID: 3, BEVERAGE_NAME: "Milk Tea", IMAGE_PATH: "/Assets/milk_tea.jpg", CATEGORY_ID: 2, STATUS: 1 },
        { ID: 4, BEVERAGE_NAME: "Orange Juice", IMAGE_PATH: "/Assets/orange_juice.jpg", CATEGORY_ID: 3, STATUS: 1 },
        { ID: 5, BEVERAGE_NAME: "Espresso", IMAGE_PATH: "/Assets/espresso.jpg", CATEGORY_ID: 1, STATUS: 1 },
        { ID: 6, BEVERAGE_NAME: "Black Coffee", IMAGE_PATH: "/Assets/black_coffee.jpg", CATEGORY_ID: 1, STATUS: 1 },
        { ID: 7, BEVERAGE_NAME: "Oolong Tea", IMAGE_PATH: "/Assets/oolong_tea.jpg", CATEGORY_ID: 2, STATUS: 1 },
        { ID: 8, BEVERAGE_NAME: "Apple Juice", IMAGE_PATH: "/Assets/apple_juice.jpg", CATEGORY_ID: 3, STATUS: 1 },
        { ID: 9, BEVERAGE_NAME: "Smoothie", IMAGE_PATH: "/Assets/smoothie.jpg", CATEGORY_ID: 4, STATUS: 1 },
        { ID: 10, BEVERAGE_NAME: "Iced Latte", IMAGE_PATH: "/Assets/iced_latte.jpg", CATEGORY_ID: 1, STATUS: 1 },
        { ID: 11, BEVERAGE_NAME: "Matcha Latte", IMAGE_PATH: "/Assets/matcha_latte.jpg", CATEGORY_ID: 2, STATUS: 1 },
        { ID: 12, BEVERAGE_NAME: "Lemon Tea", IMAGE_PATH: "/Assets/lemon_tea.jpg", CATEGORY_ID: 2, STATUS: 1 },
        { ID: 13, BEVERAGE_NAME: "Pineapple Juice", IMAGE_PATH: "/Assets/pineapple_juice.jpg", CATEGORY_ID: 3, STATUS: 1 },
        { ID: 14, BEVERAGE_NAME: "Mango Smoothie", IMAGE_PATH: "/Assets/mango_smoothie.jpg", CATEGORY_ID: 4, STATUS: 1 },
        { ID: 15, BEVERAGE_NAME: "Cappuccino", IMAGE_PATH: "/Assets/cappuccino.jpg", CATEGORY_ID: 1, STATUS: 1 },
        { ID: 16, BEVERAGE_NAME: "Cola", IMAGE_PATH: "/Assets/cola.jpg", CATEGORY_ID: 5, STATUS: 1 },
        { ID: 17, BEVERAGE_NAME: "Lemon Soda", IMAGE_PATH: "/Assets/lemon_soda.jpg", CATEGORY_ID: 5, STATUS: 1 },
        { ID: 18, BEVERAGE_NAME: "Sparkling Water", IMAGE_PATH: "/Assets/sparkling_water.jpg", CATEGORY_ID: 6, STATUS: 1 },
        { ID: 19, BEVERAGE_NAME: "Mineral Water", IMAGE_PATH: "/Assets/mineral_water.jpg", CATEGORY_ID: 6, STATUS: 1 },
        { ID: 20, BEVERAGE_NAME: "Chocolate Milkshake", IMAGE_PATH: "/Assets/chocolate_milkshake.jpg", CATEGORY_ID: 7, STATUS: 1 },
        { ID: 21, BEVERAGE_NAME: "Vanilla Milkshake", IMAGE_PATH: "/Assets/vanilla_milkshake.jpg", CATEGORY_ID: 7, STATUS: 1 },
        { ID: 22, BEVERAGE_NAME: "Strawberry Milkshake", IMAGE_PATH: "/Assets/strawberry_milkshake.jpg", CATEGORY_ID: 7, STATUS: 1 }
      ]);
    });
};
