import React, { useRef } from 'react';
import { MuiPickersUtilsProvider, KeyboardDatePicker } from '@material-ui/pickers';
import { Box, FormHelperText } from '@material-ui/core';
import DayjsUtils from '@date-io/dayjs';
import FieldWrapper from 'components/FieldWrapper/FieldWrapper';

import useStyles from './styles';

function DateQuestion({ question, formik }) {
  const classes = useStyles();
  const datePicker = useRef();
  const { touched, values, errors, setFieldValue } = formik;
  const { ID, Text: title, IsRequired: isRequired = false } = question;

  const handleDateChange = date => {
    setFieldValue(ID, date);
  };

  const handleError = () => {};

  return (
    <>
      <FieldWrapper label={title} isRequired={isRequired}>
        <div className={classes.dateWrapper}>
          <div className={classes.datePicker} ref={datePicker}>
            <MuiPickersUtilsProvider utils={DayjsUtils}>
              <KeyboardDatePicker
                InputProps={{ readOnly: true }}
                autoFocus
                variant="inline"
                format="MM/DD/YYYY"
                margin="normal"
                id="date-picker-inline"
                value={values[ID] || new Date()}
                onError={handleError}
                onChange={handleDateChange}
                autoOk
                classes={{
                  root: classes.dateInput
                }}
                PopoverProps={{
                  anchorEl: datePicker.current,
                  anchorOrigin: {
                    horizontal: 150,
                    vertical: 'bottom'
                  }
                }}
                KeyboardButtonProps={{
                  'aria-label': 'change date'
                }}
              />
            </MuiPickersUtilsProvider>
          </div>
        </div>
      </FieldWrapper>
      {touched[ID] && (
        <Box className={classes.flexCenter}>
          <FormHelperText className={classes.maxWidth} error>
            {errors[ID]}
          </FormHelperText>
        </Box>
      )}
    </>
  );
}

export default DateQuestion;
