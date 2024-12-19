// seeds/013_shift_attendance_seed.js
exports.seed = function(knex) {
    return knex('SHIFT_ATTENDANCE').del()
      .then(function() {
        return knex('SHIFT_ATTENDANCE').insert([
          { ID: 1, EMPLOYEE_ID: 1, SHIFT_DATE: '2024-12-01', MORNING_SHIFT: true, AFTERNOON_SHIFT: false, NOTE: 'Attended morning shift' },
          { ID: 2, EMPLOYEE_ID: 2, SHIFT_DATE: '2024-12-01', MORNING_SHIFT: false, AFTERNOON_SHIFT: true, NOTE: 'Attended afternoon shift' },
          { ID: 3, EMPLOYEE_ID: 3, SHIFT_DATE: '2024-12-01', MORNING_SHIFT: true, AFTERNOON_SHIFT: true, NOTE: 'Attended both shifts' },
          { ID: 4, EMPLOYEE_ID: 4, SHIFT_DATE: '2024-12-02', MORNING_SHIFT: true, AFTERNOON_SHIFT: false, NOTE: 'Attended morning shift' },
          { ID: 5, EMPLOYEE_ID: 5, SHIFT_DATE: '2024-12-02', MORNING_SHIFT: false, AFTERNOON_SHIFT: true, NOTE: 'Attended afternoon shift' },
          { ID: 6, EMPLOYEE_ID: 1, SHIFT_DATE: '2024-12-02', MORNING_SHIFT: true, AFTERNOON_SHIFT: true, NOTE: 'Attended both shifts' },
          { ID: 7, EMPLOYEE_ID: 2, SHIFT_DATE: '2024-12-03', MORNING_SHIFT: true, AFTERNOON_SHIFT: false, NOTE: 'Attended morning shift' },
          { ID: 8, EMPLOYEE_ID: 3, SHIFT_DATE: '2024-12-03', MORNING_SHIFT: false, AFTERNOON_SHIFT: true, NOTE: 'Attended afternoon shift' },
          { ID: 9, EMPLOYEE_ID: 4, SHIFT_DATE: '2024-12-03', MORNING_SHIFT: true, AFTERNOON_SHIFT: true, NOTE: 'Attended both shifts' },
          { ID: 10, EMPLOYEE_ID: 5, SHIFT_DATE: '2024-12-04', MORNING_SHIFT: true, AFTERNOON_SHIFT: false, NOTE: 'Attended morning shift' }
        ]);
      });
  };