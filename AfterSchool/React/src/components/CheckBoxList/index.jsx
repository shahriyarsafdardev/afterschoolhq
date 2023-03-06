import { FormControlLabel, Checkbox, Grid } from '@material-ui/core';
import FieldWrapper from 'components/FieldWrapper/FieldWrapper';
import React, { useState, useEffect } from 'react';
import CheckIcon from '@material-ui/icons/Check';
import useStyles from './styles';

function CheckBoxList({ formik, question }) {
  const classes = useStyles();
  const { values, setFieldValue } = formik;
  const { ID, Options, Text: title, isRequired } = question;
  const [checkBoxListValue, setCheckBoxListValue] = useState(undefined);

  const isChecked = (arr, value) => {
    return arr.some(option => option.value === value);
  };
  const options = Options.split(',').map(option => ({ key: option, value: option }));

  const updateSelectedValues = value => {
    if (isChecked(checkBoxListValue, value))
      setCheckBoxListValue(checkBoxListValue.filter(option => option.value !== value));
    else setCheckBoxListValue([...checkBoxListValue, { key: value, value }]);
  };

  const handleCheck = ({ target: { value } }) => {
    updateSelectedValues(value);
  };

  useEffect(() => {
    if (checkBoxListValue === undefined || !checkBoxListValue.length) {
      if (['undefined'].includes(typeof values[ID]) || !values[ID]) setCheckBoxListValue([]);
      else {
        const obj = JSON.parse(values[ID]);
        setCheckBoxListValue(obj[0] instanceof Array ? obj[0] : obj);
      }
    }
  }, [values[ID]]);

  useEffect(() => setFieldValue(ID, JSON.stringify(checkBoxListValue)), [checkBoxListValue]);
  if (!checkBoxListValue) return <div></div>;

  return (
    <FieldWrapper label={title} isRequired={isRequired}>
      <Grid xs={12} md={8} item className={classes.textFieldGrid}>
        <div className={classes.checkboxWrapper}>
          <>
            {options.length &&
              options.map(({ key, value }) => (
                <FormControlLabel
                  key={key}
                  control={
                    <Checkbox
                      checked={checkBoxListValue ? isChecked(checkBoxListValue, value) : false}
                      value={value}
                      onChange={handleCheck}
                      classes={{
                        checked: classes.checked,
                        root: classes.root
                      }}
                      checkedIcon={
                        <CheckIcon
                          style={{
                            height: '18px',
                            width: '18px'
                          }}
                        />
                      }
                      IconStyle={{ fill: 'black' }}
                    />
                  }
                  label={value}
                  classes={{
                    root: classes.formControlLabelRoot
                  }}
                />
              ))}
          </>
        </div>
      </Grid>
    </FieldWrapper>
  );
}

export default CheckBoxList;
