// migrations/xxxx_create_recipe.js
exports.up = function(knex) {
    return knex.schema.createTable('RECIPE', (table) => {
      table.integer('BEVERAGE_SIZE_ID').unsigned().references('ID').inTable('BEVERAGE_SIZE');
      table.integer('MATERIAL_ID').unsigned().references('ID').inTable('MATERIAL');
      table.integer('QUANTITY');
      table.primary(['BEVERAGE_SIZE_ID', 'MATERIAL_ID']);
    });
  };
  
  exports.down = function(knex) {
    return knex.schema.dropTableIfExists('RECIPE');
  };
  