// seeds/type_beverage_seed.js
exports.seed = function(knex) {
  return knex('TYPE_BEVERAGE').del()
    .then(function() {
      return knex('TYPE_BEVERAGE').insert([
        { ID: 1, CATEGORY: 'Coffee', IMAGE_PATH: "/Assets/coffee.jpg", STATUS: 1 },
        { ID: 2, CATEGORY: 'Tea', IMAGE_PATH: "/Assets/green_tea.jpg", STATUS: 1 },
        { ID: 3, CATEGORY: 'Juice', IMAGE_PATH: "/Assets/pineapple_juice.jpg", STATUS: 1 },
        { ID: 4, CATEGORY: 'Smoothie', IMAGE_PATH: "/Assets/smoothie.jpg", STATUS: 1 },
        { ID: 5, CATEGORY: 'Soda', IMAGE_PATH: "/Assets/smoothie.jpg", STATUS: 1 },
        { ID: 6, CATEGORY: 'Water', IMAGE_PATH: "/Assets/soda.jpg", STATUS: 1 },
        { ID: 7, CATEGORY: 'Milkshake', IMAGE_PATH: "/Assets/milkshake.jpg", STATUS: 1 }
      ]);
    });
};
