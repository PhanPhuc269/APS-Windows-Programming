// seeds/type_beverage_seed.js
exports.seed = function(knex) {
  return knex('TYPE_BEVERAGE').del()
    .then(function() {
      return knex('TYPE_BEVERAGE').insert([
        { ID: 1, CATEGORY: 'Coffee', IMAGE_PATH: "/Assets/coffee.jpg" },
        { ID: 2, CATEGORY: 'Tea', IMAGE_PATH: "/Assets/green_tea.jpg" },
        { ID: 3, CATEGORY: 'Juice', IMAGE_PATH: "/Assets/pineapple_juice.jpg" },
        { ID: 4, CATEGORY: 'Smoothie', IMAGE_PATH: "/Assets/smoothie.jpg" },
        { ID: 5, CATEGORY: 'Soda', IMAGE_PATH: "/Assets/smoothie.jpg" },
        { ID: 6, CATEGORY: 'Water', IMAGE_PATH: "/Assets/soda.jpg" },
        { ID: 7, CATEGORY: 'Milkshake', IMAGE_PATH: "/Assets/milkshake.jpg" }
      ]);
    });
};