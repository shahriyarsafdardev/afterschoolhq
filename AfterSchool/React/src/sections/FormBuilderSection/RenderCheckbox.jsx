import React from 'react';
import CheckIcon from '@material-ui/icons/Check';
import { makeStyles, Checkbox } from '@material-ui/core';

function RenderCheckbox({ checked = false, handleCheck }) {
  const classes = makeStyles(() => ({
    checkboxWrapper: {
      padding: '0px 12px'
    },
    formControlLabelRoot: {
      margin: '10px 0px',
      '& .MuiTypography-root': {
        marginLeft: 18
      }
    },
    checked: {
      color: '#494949 !important',
      padding: 0,
      borderRadius: 3,
      background: 'transparent !important'
    },
    root: {
      padding: 0,
      background: '#ffffff',
      color: '#ffffff',
      border: '1px solid #c4c4c4',
      borderRadius: 3,
      height: '18px',
      width: '18px',
      '& .MuiSvgIcon-root': {
        height: '18px',
        width: '18px'
      }
    }
  }))();

  return (
    <Checkbox
      data-testid={`${'title'}-${'QuestionDataTypeId'}-checkbox`}
      checked={checked}
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
  );
}

export default RenderCheckbox;
