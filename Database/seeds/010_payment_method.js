// seeds/payment_method_seed.js
exports.seed = function(knex) {
  return knex('PAYMENT_METHOD').del()
    .then(function() {
      return knex('PAYMENT_METHOD').insert([
        { ID: 1, METHOD_NAME: 'Credit Card' },
        { ID: 2, METHOD_NAME: 'Debit Card' },
        { ID: 3, METHOD_NAME: 'PayPal' },
        { ID: 4, METHOD_NAME: 'Cash' },
        { ID: 5, METHOD_NAME: 'QR Code MoMo' }
      ]);
    });
};