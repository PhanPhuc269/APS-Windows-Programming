// migrations/xxxx_create_beverage_size.js
exports.up = function(knex) {
    return knex.schema.createTable('BEVERAGE_SIZE', (table) => {
      table.increments('ID');
      table.integer('BEVERAGE_ID').unsigned().references('ID').inTable('BEVERAGE');
      table.string('SIZE', 1);
      table.decimal('PRICE', 10, 2);
    });
  };
  
  exports.down = function(knex) {
    return knex.schema.dropTableIfExists('BEVERAGE_SIZE');
  };