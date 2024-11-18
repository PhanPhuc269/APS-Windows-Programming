// seeds/access_management_seed.js
exports.seed = function(knex) {
  return knex('ACCESS_MANAGEMENT').del()
    .then(function() {
      return knex('ACCESS_MANAGEMENT').insert([
        { LOGIN_ID: 1, ACCOUNT_ID: 1, LOGIN_TIME: '2024-10-31 08:00:00', LOGOUT_TIME: '2024-10-31 16:00:00' },
        { LOGIN_ID: 2, ACCOUNT_ID: 2, LOGIN_TIME: '2024-10-31 09:00:00', LOGOUT_TIME: '2024-10-31 17:00:00' }
      ]);
    });
};
