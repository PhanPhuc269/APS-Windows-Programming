// seeds/payment_method_seed.js
exports.seed = function(knex) {
  return knex('PAYMENT_METHOD').del()
    .then(function() {
      return knex('PAYMENT_METHOD').insert([
        { ID: 1, METHOD_NAME: 'Credit Card' },
        { ID: 2, METHOD_NAME: 'VietQR' },
        { ID: 3, METHOD_NAME: 'VNPay' },
        { ID: 4, METHOD_NAME: 'Cash' },
        { ID: 5, METHOD_NAME: 'MoMo' }
      ]);
    });
};