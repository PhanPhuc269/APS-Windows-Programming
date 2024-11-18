// migrations/xxxx_create_payment_method.js
exports.up = function(knex) {
    return knex.schema.createTable('PAYMENT_METHOD', (table) => {
      table.increments('ID');
      table.string('METHOD_NAME', 50).notNullable();
    });
  };
  
  exports.down = function(knex) {
    return knex.schema.dropTableIfExists('PAYMENT_METHOD');
  };