// migrations/xxxx_create_orders.js
exports.up = function(knex) {
    return knex.schema.createTable('ORDERS', (table) => {
      table.increments('ORDER_ID').primary();
      table.integer('CUSTOMER_ID').unsigned().references('CUSTOMER_ID').inTable('CUSTOMERS');
      table.integer('RESERVED_TABLE_ID').unsigned().references('RESERVED_TABLE_ID').inTable('RESERVED_TABLE');
      table.datetime('ORDER_TIME');
      table.datetime('COMPLETED_TIME');
      table.integer('TOTAL_AMOUNT');
      table.string('PAYMENT_METHOD');
      table.integer('CONSUMED_POINTS');
      table.integer('AMOUNT_DUE');
      table.integer('EMPLOYEE_ID').unsigned().references('EMPLOYEE_ID').inTable('ACCOUNT');
    });
  };
  
  exports.down = function(knex) {
    return knex.schema.dropTableIfExists('ORDERS');
  };
