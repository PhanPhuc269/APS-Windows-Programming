// seeds/002_account_seed.js
exports.seed = function(knex) {
  return knex('ACCOUNT').del()
    .then(function() {
      return knex('ACCOUNT').insert([
        { EMPLOYEE_ID: 1, EMP_NAME: 'Admin User', EMP_ROLE: 'Admin', ACCESS_LEVEL: 1, USERNAME: 'admin', USER_PASSWORD: 'password' },
        { EMPLOYEE_ID: 2, EMP_NAME: 'User One', EMP_ROLE: 'Staff', ACCESS_LEVEL: 2, USERNAME: 'user1', USER_PASSWORD: 'password' },
        { EMPLOYEE_ID: 3, EMP_NAME: 'User Two', EMP_ROLE: 'Staff', ACCESS_LEVEL: 2, USERNAME: 'user2', USER_PASSWORD: 'password' },
        { EMPLOYEE_ID: 4, EMP_NAME: 'User Three', EMP_ROLE: 'Staff', ACCESS_LEVEL: 2, USERNAME: 'user3', USER_PASSWORD: 'password' },
        { EMPLOYEE_ID: 5, EMP_NAME: 'User Four', EMP_ROLE: 'Staff', ACCESS_LEVEL: 2, USERNAME: 'user4', USER_PASSWORD: 'password' }
      ]);
    });
};