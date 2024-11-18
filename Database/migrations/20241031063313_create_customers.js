// migrations/xxxx_create_customers.js
exports.up = function(knex) {
    return knex.schema.createTable('CUSTOMERS', (table) => {
      table.increments('CUSTOMER_ID');
      table.string('CUSTOMER_NAME', 40).notNullable();
      table.string('PHONE_NUMBER', 10);
      table.string('EMAIL', 70);
      table.integer('POINTS').defaultTo(0);
      table.string('REWARDS', 70);
    });
  };
  
  exports.down = function(knex) {
    return knex.schema.dropTableIfExists('CUSTOMERS');
  };
  