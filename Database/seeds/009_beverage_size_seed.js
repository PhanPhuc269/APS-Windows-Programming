// seeds/beverage_size_seed.js
exports.seed = function(knex) {
  return knex('BEVERAGE_SIZE').del()
    .then(function() {
      return knex('BEVERAGE_SIZE').insert([
        { BEVERAGE_ID: 1, SIZE: 'M', PRICE: 45000 },
        { BEVERAGE_ID: 1, SIZE: 'L', PRICE: 50000 },
        { BEVERAGE_ID: 2, SIZE: 'M', PRICE: 35000 },
        { BEVERAGE_ID: 2, SIZE: 'L', PRICE: 38000 },
        { BEVERAGE_ID: 3, SIZE: 'M', PRICE: 30000 },
        { BEVERAGE_ID: 3, SIZE: 'L', PRICE: 35000 },
        { BEVERAGE_ID: 4, SIZE: 'M', PRICE: 27000 },
        { BEVERAGE_ID: 4, SIZE: 'L', PRICE: 30000 },
        { BEVERAGE_ID: 5, SIZE: 'M', PRICE: 45000 },
        { BEVERAGE_ID: 6, SIZE: 'M', PRICE: 37000 },
        { BEVERAGE_ID: 6, SIZE: 'L', PRICE: 40000 },
        { BEVERAGE_ID: 7, SIZE: 'M', PRICE: 35000 },
        { BEVERAGE_ID: 7, SIZE: 'L', PRICE: 38000 },
        { BEVERAGE_ID: 8, SIZE: 'M', PRICE: 29000 },
        { BEVERAGE_ID: 8, SIZE: 'L', PRICE: 31000 },
        { BEVERAGE_ID: 9, SIZE: 'M', PRICE: 45000 },
        { BEVERAGE_ID: 9, SIZE: 'L', PRICE: 50000 },
        { BEVERAGE_ID: 10, SIZE: 'M', PRICE: 47000 },
        { BEVERAGE_ID: 10, SIZE: 'L', PRICE: 50000 },
        { BEVERAGE_ID: 11, SIZE: 'M', PRICE: 48000 },
        { BEVERAGE_ID: 11, SIZE: 'L', PRICE: 51000 },
        { BEVERAGE_ID: 12, SIZE: 'M', PRICE: 32000 },
        { BEVERAGE_ID: 12, SIZE: 'L', PRICE: 35000 },
        { BEVERAGE_ID: 13, SIZE: 'M', PRICE: 29000 },
        { BEVERAGE_ID: 13, SIZE: 'L', PRICE: 31000 },
        { BEVERAGE_ID: 14, SIZE: 'M', PRICE: 47000 },
        { BEVERAGE_ID: 14, SIZE: 'L', PRICE: 50000 },
        { BEVERAGE_ID: 15, SIZE: 'M', PRICE: 45000 },
        { BEVERAGE_ID: 15, SIZE: 'L', PRICE: 48000 },
        { BEVERAGE_ID: 16, SIZE: 'M', PRICE: 20000 },
        { BEVERAGE_ID: 16, SIZE: 'L', PRICE: 25000 },
        { BEVERAGE_ID: 17, SIZE: 'M', PRICE: 22000 },
        { BEVERAGE_ID: 17, SIZE: 'L', PRICE: 27000 },
        { BEVERAGE_ID: 18, SIZE: 'M', PRICE: 15000 },
        { BEVERAGE_ID: 18, SIZE: 'L', PRICE: 20000 },
        { BEVERAGE_ID: 19, SIZE: 'M', PRICE: 12000 },
        { BEVERAGE_ID: 19, SIZE: 'L', PRICE: 17000 },
        { BEVERAGE_ID: 20, SIZE: 'M', PRICE: 30000 },
        { BEVERAGE_ID: 20, SIZE: 'L', PRICE: 35000 },
        { BEVERAGE_ID: 21, SIZE: 'M', PRICE: 30000 },
        { BEVERAGE_ID: 21, SIZE: 'L', PRICE: 35000 },
        { BEVERAGE_ID: 22, SIZE: 'M', PRICE: 30000 },
        { BEVERAGE_ID: 22, SIZE: 'L', PRICE: 35000 }
      ]);
    });
};