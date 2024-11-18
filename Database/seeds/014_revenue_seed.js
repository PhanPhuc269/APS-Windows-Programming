// seeds/revenue_seed.js
exports.seed = function(knex) {
  return knex('REVENUE').del()
    .then(function() {
      return knex('REVENUE').insert([
        { REVENUE_ID: 1, ORDER_QUANTITY: 5, REVENUE_DATE: '2024-11-01', TOTAL_AMOUNT: 900000 },
        { REVENUE_ID: 2, ORDER_QUANTITY: 5, REVENUE_DATE: '2024-11-02', TOTAL_AMOUNT: 950000 },
        { REVENUE_ID: 3, ORDER_QUANTITY: 5, REVENUE_DATE: '2024-11-03', TOTAL_AMOUNT: 1100000 }
      ]);
    });
};