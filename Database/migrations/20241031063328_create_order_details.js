// migrations/xxxx_create_order_details.js
exports.up = function(knex) {
    return knex.schema.createTable('ORDER_DETAILS', (table) => {
      table.increments('ORDER_DETAILS_ID');
      table.integer('ORDER_ID').unsigned().references('ORDER_ID').inTable('ORDERS');
      table.integer('BEVERAGE_SIZE_ID').unsigned().references('ID').inTable('BEVERAGE_SIZE');
      table.integer('QUANTITY');
      table.integer('PRICE');
      table.integer('SUBTOTAL');
    });
  };
  
  exports.down = function(knex) {
    return knex.schema.dropTableIfExists('ORDER_DETAILS');
  };
  