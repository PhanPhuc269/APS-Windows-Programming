// migrations/xxxx_create_type_beverage.js
exports.up = function(knex) {
    return knex.schema.createTable('TYPE_BEVERAGE', (table) => {
      table.increments('ID');
      table.string('CATEGORY', 40);
    });
  };
  
  exports.down = function(knex) {
    return knex.schema.dropTableIfExists('TYPE_BEVERAGE');
  };
  