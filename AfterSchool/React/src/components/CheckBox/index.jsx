import React from 'react';
import { FormControlLabel, Checkbox, Grid } from '@material-ui/core';
import CheckIcon from '@material-ui/icons/Check';
import useStyles from './styles';

function CheckBox({ formik, question }) {
  const classes = useStyles();
  const { ID, Text: title } = question;
  const { values, setFieldValue } = formik;

  const handleCheck = ({ target: { checked } }) => {
    setFieldValue(ID, checked ? 1 : 0);
  };

  return (
    <Grid className={classes.textFieldBox} xs={12}>
      <Grid xs={12} md={4} item className={classes.titleGrid} />
      <Grid xs={12} md={8} item className={classes.textFieldGrid}>
        <div className={classes.checkboxWrapper}>
          <FormControlLabel
            control={
              <Checkbox
                checked={values[ID]}
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
            label={title}
            classes={{
              root: classes.formControlLabelRoot
            }}
            onChange={handleCheck}
          />
        </div>
      </Grid>
    </Grid>
  );
}

export default CheckBox;
