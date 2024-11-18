// migrations/xxxx_create_time_management.js
exports.up = function(knex) {
    return knex.schema.createTable('TIME_MANAGEMENT', (table) => {
      table.integer('ID').primary();
      table.integer('EMPLOYEE_ID').unsigned().references('EMPLOYEE_ID').inTable('ACCOUNT');
      table.datetime('TIME_START');
      table.datetime('TIME_END');
    });
  };
  
  exports.down = function(knex) {
    return knex.schema.dropTableIfExists('TIME_MANAGEMENT');
  };
  