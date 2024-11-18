// migrations/xxxx_create_reserved_table.js
exports.up = function(knex) {
    return knex.schema.createTable('RESERVED_TABLE', (table) => {
      table.increments('RESERVED_TABLE_ID');
      table.integer('TABLE_ID');
      table.boolean('IS_RESERVED');
      table.datetime('RESERVATION_TIME');
      table.integer('SEATS');
    });
  };
  
  exports.down = function(knex) {
    return knex.schema.dropTableIfExists('RESERVED_TABLE');
  };
  