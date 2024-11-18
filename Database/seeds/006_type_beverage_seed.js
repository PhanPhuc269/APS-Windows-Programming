// seeds/type_beverage_seed.js
exports.seed = function(knex) {
  return knex('TYPE_BEVERAGE').del()
    .then(function() {
      return knex('TYPE_BEVERAGE').insert([
        { ID: 1, CATEGORY: 'Coffee' },
        { ID: 2, CATEGORY: 'Tea' },
        { ID: 3, CATEGORY: 'Juice' },
        { ID: 4, CATEGORY: 'Smoothie' },
        { ID: 5, CATEGORY: 'Soda' },
        { ID: 6, CATEGORY: 'Water' },
        { ID: 7, CATEGORY: 'Milkshake' }
      ]);
    });
};