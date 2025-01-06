exports.up = function (knex) {
    return knex.schema.createTable('SHIFT_ATTENDANCE', (table) => {
        table.increments('ID'); // ID tự tăng
        table.integer('EMPLOYEE_ID').unsigned().notNullable().references('EMPLOYEE_ID').inTable('ACCOUNT'); // Nhân viên
        table.date('SHIFT_DATE').notNullable(); // Ngày chấm công
        table.boolean('MORNING_SHIFT').defaultTo(false); // Trạng thái chấm công ca sáng
        table.boolean('AFTERNOON_SHIFT').defaultTo(false); // Trạng thái chấm công ca chiều
        table.text('NOTE'); // Ghi chú thêm (nếu có)
        table.timestamps(true, true); // Thời gian tạo và cập nhật
    });
};

exports.down = function (knex) {
    return knex.schema.dropTableIfExists('SHIFT_ATTENDANCE');
};
