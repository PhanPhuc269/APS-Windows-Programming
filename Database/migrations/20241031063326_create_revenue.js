// migrations/xxxx_create_revenue.js
exports.up = function(knex) {
    return knex.schema.createTable('REVENUE', (table) => {
      table.integer('REVENUE_ID').primary();
      table.integer('ORDER_QUANTITY')
      table.datetime('REVENUE_DATE');
      table.integer('TOTAL_AMOUNT');
    });
  };
  
  exports.down = function(knex) {
    return knex.schema.dropTableIfExists('REVENUE');
  };
  