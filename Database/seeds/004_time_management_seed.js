// seeds/time_management_seed.js
exports.seed = function(knex) {
  return knex('TIME_MANAGEMENT').del()
    .then(function() {
      return knex('TIME_MANAGEMENT').insert([
        { ID: 1, EMPLOYEE_ID: 1, TIME_START: '2024-10-31 08:00:00', TIME_END: '2024-10-31 16:00:00' },
        { ID: 2, EMPLOYEE_ID: 2, TIME_START: '2024-10-31 09:00:00', TIME_END: '2024-10-31 17:00:00' }
      ]);
    });
};
