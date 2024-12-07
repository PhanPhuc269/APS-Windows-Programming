// migrations/xxxx_create_material.js
exports.up = function(knex) {
    return knex.schema.createTable('MATERIAL', (table) => {
      table.increments('ID');
      table.string('MATERIAL_CODE', 10).unique().notNullable();
      table.string('MATERIAL_NAME', 40).notNullable();
      table.integer('QUANTITY').notNullable();
      table.string('CATEGORY', 20).notNullable();
      table.string('UNIT', 10).notNullable();
      table.integer('UNIT_PRICE').notNullable();
      table.datetime('IMPORT_DATE').notNullable();
      table.datetime('EXPIRATION_DATE').notNullable();
      
    });
  };
  
  exports.down = function(knex) {
    return knex.schema.dropTableIfExists('MATERIAL');
  };