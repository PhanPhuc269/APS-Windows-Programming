// migrations/xxxx_create_access_management.js
exports.up = function(knex) {
    return knex.schema.createTable('ACCESS_MANAGEMENT', (table) => {
      table.integer('LOGIN_ID').primary();
      table.integer('ACCOUNT_ID').unsigned().references('EMPLOYEE_ID').inTable('ACCOUNT');
      table.datetime('LOGIN_TIME');
      table.datetime('LOGOUT_TIME');
    });
  };
  
  exports.down = function(knex) {
    return knex.schema.dropTableIfExists('ACCESS_MANAGEMENT');
  };
  