// migrations/xxxx_create_beverage.js
exports.up = function(knex) {
    return knex.schema.createTable('BEVERAGE', (table) => {
      table.increments('ID');
      table.string('BEVERAGE_NAME', 40);
      table.integer('CATEGORY_ID').unsigned().references('ID').inTable('TYPE_BEVERAGE');
      table.string('IMAGE_PATH');
      table.int('STATUS');
    });
  };
  
  exports.down = function(knex) {
    return knex.schema.dropTableIfExists('BEVERAGE');
  };
  