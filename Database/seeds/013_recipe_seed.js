// seeds/recipe_seed.js
exports.seed = function(knex) {
  return knex('RECIPE').del()
    .then(function() {
      return knex('RECIPE').insert([
        { BEVERAGE_SIZE_ID: 1, MATERIAL_ID: 1, QUANTITY: 10 },
        { BEVERAGE_SIZE_ID: 1, MATERIAL_ID: 2, QUANTITY: 5 },
        { BEVERAGE_SIZE_ID: 2, MATERIAL_ID: 1, QUANTITY: 12 },
        { BEVERAGE_SIZE_ID: 2, MATERIAL_ID: 3, QUANTITY: 6 },
        { BEVERAGE_SIZE_ID: 3, MATERIAL_ID: 2, QUANTITY: 8 },
        { BEVERAGE_SIZE_ID: 3, MATERIAL_ID: 4, QUANTITY: 4 },
        { BEVERAGE_SIZE_ID: 4, MATERIAL_ID: 5, QUANTITY: 7 },
        { BEVERAGE_SIZE_ID: 4, MATERIAL_ID: 6, QUANTITY: 3 },
        { BEVERAGE_SIZE_ID: 5, MATERIAL_ID: 7, QUANTITY: 9 },
        { BEVERAGE_SIZE_ID: 5, MATERIAL_ID: 8, QUANTITY: 2 }
      ]);
    });
};